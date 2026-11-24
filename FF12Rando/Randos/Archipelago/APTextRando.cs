using Bartz24.Data;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FF12Rando;
public class APTextRando : TextRando
{
    private const int ArchipelagoItemID = 0x80E6;

    // AP Text Colors
    private const string ResetColor = "{rgb:gray}";
    private const string PlayerColor = "{rgb:125,125,105}";     // yellow FAFAD2
    private const string ProgressionColor = "{rgb:88,77,120}";  // plum AF99EF
    private const string UsefulColor = "{rgb:55,70,116}";       // slateblue 6D8BE8
    private const string TrapColor = "{rgb:125,64,57}";         // salmon FA8072
    private const string FillerColor = "{rgb:0,119,119}";       // cyan 00EEEE

    private const string OwnerSeparator = "'s ";

    private const int LineWidth = 72;
    private const int ItemNameMaxLength = 60;
    // Names go into the key item banner after "You obtain ".
    private const int BannerPrefixLength = 11;
    private const int NameMaxBytes = 300;
    private const int MessageMaxBytes = 340;

    // A estimate of what one converted colour tag takes.
    private const int ColorTagBytes = 6;
    // Never a real colour, so the first word after a line break always writes its own.
    private const string UnknownColor = "\0";

    private readonly record struct Word(string Text, string Color, bool Glued);

    public APTextRando(SeedGenerator randomizers) : base(randomizers)
    {
    }

    protected override string SeedLabel => base.SeedLabel + " (Archipelago)";

    private string GetClassificationColor(int? flags)
    {
        if (flags == null)
        {
            return null;
        }

        int value = flags.Value;
        return value == 0 ? FillerColor
            : (value & 0b001) != 0 ? ProgressionColor
            : (value & 0b010) != 0 ? UsefulColor
            : (value & 0b100) != 0 ? TrapColor
            : FillerColor;
    }

    private string FormatChecks(List<APTreasureRando.ArchipelagoCheckItem> checks, string lead, int firstLineOffset, int maxBytes)
    {
        bool colored = checks.All(c => !string.IsNullOrEmpty(c.Name) && GetClassificationColor(c.Flags) != null);
        var entries = checks.Select(c => colored
            ? (Player: string.IsNullOrEmpty(c.Player) ? null : TextSanitizer.Sanitize(c.Player),
                Name: TextSanitizer.Sanitize(c.Name), Color: GetClassificationColor(c.Flags))
            : (Player: (string)null, Name: TextSanitizer.Sanitize(c.Display), Color: (string)null)).ToList();
        string leadText = lead == null ? null : TextSanitizer.Sanitize(lead);

        const int minNameLength = 2;
        for (int cap = Math.Min(entries.Max(e => TextSanitizer.Length(e.Name)), ItemNameMaxLength); ; cap--)
        {
            string text = WrapChecks(leadText, entries, cap, firstLineOffset, out int bytes);
            if (bytes <= maxBytes || cap <= minNameLength)
            {
                return text;
            }
        }
    }

    private string WrapChecks(string lead, List<(string Player, string Name, string Color)> entries, int nameCap, int firstLineOffset, out int bytes)
    {
        // Each group is one entry (or the lead), moved to the next line whole when it fits there.
        List<List<Word>> groups = new();
        if (lead != null)
        {
            groups.Add(lead.Split(' ').Select(w => new Word(w, null, false)).ToList());
        }

        foreach (var entry in entries)
        {
            if (groups.Count > 0)
            {
                groups[^1].Add(new Word(",", null, true));
            }

            List<Word> words = new();
            if (entry.Player != null)
            {
                // The "'s" takes the player's colour; the item's colour that follows ends it.
                words.AddRange((entry.Player + OwnerSeparator.TrimEnd()).Split(' ').Select(w => new Word(w, PlayerColor, false)));
            }

            words.AddRange(TextSanitizer.Truncate(entry.Name, nameCap).Split(' ').Select(w => new Word(w, entry.Color, false)));
            groups.Add(words);
        }

        StringBuilder result = new();
        string current = null;
        int lineLength = firstLineOffset;
        bool lineHasText = false;
        int byteCount = 0;

        void Append(string text)
        {
            result.Append(text);
            byteCount += Encoding.UTF8.GetByteCount(text);
        }

        void NewLine()
        {
            Append("\n");
            lineLength = 0;
            lineHasText = false;
            current = current == null ? null : UnknownColor;
        }

        foreach (List<Word> group in groups)
        {
            bool space = result.Length > 0;
            int width = group.Sum(w => TextSanitizer.Length(w.Text)) + group.Count(w => !w.Glued) - 1;
            if (lineHasText && lineLength + 1 + width > LineWidth && width <= LineWidth)
            {
                NewLine();
                space = false;
            }

            for (int i = 0; i < group.Count; i++)
            {
                Word word = group[i];
                bool wordSpace = i == 0 ? space : !word.Glued;
                // Glued words (the commas) stay on the line of the word before them.
                int length = TextSanitizer.Length(word.Text)
                    + group.Skip(i + 1).TakeWhile(w => w.Glued).Sum(w => TextSanitizer.Length(w.Text));
                if (wordSpace && lineHasText && lineLength + 1 + length > LineWidth)
                {
                    NewLine();
                    wordSpace = false;
                }

                if (wordSpace)
                {
                    Append(" ");
                    lineLength++;
                }

                if (word.Color != current)
                {
                    result.Append(word.Color ?? ResetColor);
                    byteCount += ColorTagBytes;
                    current = word.Color;
                }

                Append(word.Text);
                lineLength += TextSanitizer.Length(word.Text);
                lineHasText = true;
            }
        }

        if (current != null)
        {
            result.Append(ResetColor);
            byteCount += ColorTagBytes;
        }

        bytes = byteCount;
        return result.ToString();
    }

    protected override void AddRuntimeTextOverrides()
    {
        base.AddRuntimeTextOverrides();
        AddArchipelagoChestOverrides();
        AddArchipelagoRewardOverrides();
    }

    private void AddArchipelagoChestOverrides()
    {
        APTreasureRando treasureRando = (APTreasureRando)Generator.Get<TreasureRando>();
        foreach (TreasureLocation location in treasureRando.ItemLocations.Values.OfType<TreasureLocation>())
        {
            APTreasureRando.ArchipelagoCheckItem item = treasureRando.GetArchipelagoItem(location);
            if (item == null || location.GetItem(false)?.Item1 != APTreasureRando.ArchipelagoItemID)
            {
                continue;
            }

            // The respawn id is the treasure's index in its map (TreasureRando.SetTreasureRespawns).
            ChestNameOverrides[(location.MapID, location.Index, ArchipelagoItemID)] =
                FormatChecks(new() { item }, null, BannerPrefixLength, NameMaxBytes);
        }
    }

    private void AddArchipelagoRewardOverrides()
    {
        APTreasureRando treasureRando = (APTreasureRando)Generator.Get<TreasureRando>();
        foreach (var group in treasureRando.ItemLocations.Values.OfType<RewardLocation>().GroupBy(r => r.IntID))
        {
            List<APTreasureRando.ArchipelagoCheckItem> checks = group.OrderBy(r => r.Index)
                .Select(r => treasureRando.GetArchipelagoItem(r))
                .Where(item => item != null)
                .ToList();
            if (checks.Count == 0)
            {
                continue;
            }

            DataStoreReward reward = group.First().GetItemData(false);
            if (reward.Item1ID == ArchipelagoItemID || reward.Item2ID == ArchipelagoItemID)
            {
                RewardNameOverrides[(group.Key, ArchipelagoItemID)] = FormatChecks(checks, null, BannerPrefixLength, NameMaxBytes);
                continue;
            }

            int? announcedItem;
            string lead;
            if (reward.Gil > 0)
            {
                announcedItem = null;
                lead = $"{reward.Gil} Gil";
            }
            else if (reward.Item1ID != 0xFFFF || reward.Item2ID != 0xFFFF)
            {
                bool first = reward.Item1ID != 0xFFFF;
                ushort id = first ? reward.Item1ID : reward.Item2ID;
                int amount = first ? reward.Item1Amount : reward.Item2Amount;
                announcedItem = id;
                lead = treasureRando.GetItemName(id.ToString("X4")) + (amount > 1 ? $" x{amount}" : "");
            }
            else
            {
                // Nothing to announce, so nothing to hang the checks on.
                continue;
            }

            RewardMessageOverrides[(group.Key, announcedItem)] =
                FormatChecks(checks, "Obtained " + lead, 0, MessageMaxBytes);
        }
    }

    protected override void UpdateGoalInfoStr()
    {
        DataStoreBinText.StringData goalInfoStr = TextEbpZones["rbn_a16"].Values.First(v => v.Text != null && v.Text.Contains("$INFO$"));

        goalInfoStr.Text = goalInfoStr.Text.Replace("$INFO$", "The goal for this seed:\n" +
            "Find a {color:gold}Writ of Transit{rgb:gray} and travel to {italic}Bahamut{/italic}\n" +
            "using the {italic}Strahl{/italic}, accessed with the Systems Access Key");
    }
}
