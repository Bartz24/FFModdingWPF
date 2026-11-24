using Bartz24.Data;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando;

public class TextRando : Randomizer
{
    public DataStoreBinText TextAbilities = new();
    public DataStoreBinText TextEquipment = new();
    public DataStoreBinText TextLicenses = new();
    public DataStoreBinText TextKeyItems = new();
    public DataStoreBinText TextLoot = new();
    public DataStoreBinText TextLicenseAugmentDesc = new();
    public DataStoreBinText TextMenuMessage = new();
    public DataStoreBinText TextMenuCommand = new();
    public DataStoreBinText TextAbilityHelp = new();
    public DataStoreBinText TextKeyDescriptions = new();

    public Dictionary<string, DataStoreEbpBinText> TextEbpZones = new();

    protected virtual string SeedLabel => SetupData.Seed.Clean();

    // Runtime text overrides applied by RandoText.lua.
    /// <summary>Keep any choice markup the original text had; the script still expects it.</summary>
    public Dictionary<(string Map, int Index), string> DialogOverrides { get; set; } = new();
    /// <summary>Menus and shops keep the real name.</summary>
    public Dictionary<int, string> ItemNameOverrides { get; set; } = new();
    /// <summary>The count isn't added, so include it in the text if it matters.</summary>
    public Dictionary<int, string> ItemMessageOverrides { get; set; } = new();
    /// <summary>A null ItemID targets the gil line. Takes priority over <see cref="ItemMessageOverrides"/>.</summary>
    public Dictionary<(int RewardID, int? ItemID), string> RewardMessageOverrides { get; set; } = new();
    /// <summary>Takes priority over <see cref="ItemMessageOverrides"/>.</summary>
    public Dictionary<(string Map, int Respawn, int ItemID), string> ChestMessageOverrides { get; set; } = new();
    /// <summary>Also covers hunt key item banners. Takes priority over <see cref="ItemNameOverrides"/>.</summary>
    public Dictionary<(int RewardID, int ItemID), string> RewardNameOverrides { get; set; } = new();
    /// <summary>Takes priority over <see cref="ItemNameOverrides"/>.</summary>
    public Dictionary<(string Map, int Respawn, int ItemID), string> ChestNameOverrides { get; set; } = new();
    public TextRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Text Data...");
        TextAbilities.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_061.bin.dir\\section_000.bin");
        TextEquipment.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_061.bin.dir\\section_001.bin");
        TextLicenses.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_061.bin.dir\\section_003.bin");
        TextKeyItems.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_061.bin.dir\\section_012.bin");
        TextLoot.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_061.bin.dir\\section_011.bin");
        TextLicenseAugmentDesc.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_061.bin.dir\\section_010.bin");

        TextMenuMessage.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\menu_message.bin");
        TextMenuCommand.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\menu_command.bin");
        TextAbilityHelp.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\listhelp_ability.bin");
        TextKeyDescriptions.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\listhelp_daijinamono.bin");

        List<string> ebpZonesToLoad = new()
        {
            "rbn_a16",
            "fon_b01"
        };

        foreach (string ebp in Directory.GetFiles($"{Generator.DataOutFolder}\\plan_master\\us\\plan_map", "*.ebp", SearchOption.AllDirectories))
        {
            string id = Path.GetFileNameWithoutExtension(ebp);
            if (ebpZonesToLoad.Contains(id))
            {
                TextEbpZones.Add(id, new DataStoreEbpBinText());
                TextEbpZones[id].Load(ebp);
            }
        }
    }
    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Text Data...");
    }
    private string GetHash()
    {
        string numberForm = RandomNum.GetHash(6, 9);
        string iconForm = "";

        foreach (char c in numberForm)
        {
            switch (c)
            {
                case '0':
                    iconForm += "{icon:0}";
                    break;
                case '1':
                    iconForm += "{icon:10}";
                    break;
                case '2':
                    iconForm += "{icon:11}";
                    break;
                case '3':
                    iconForm += "{icon:12}";
                    break;
                case '4':
                    iconForm += "{icon:13}";
                    break;
                case '5':
                    iconForm += "{icon:14}";
                    break;
                case '6':
                    iconForm += "{icon:15}";
                    break;
                case '7':
                    iconForm += "{icon:16}";
                    break;
                case '8':
                    iconForm += "{icon:17}";
                    break;
            }
        }

        return iconForm;
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Text Data...");

        // Computed once: it reseeds the RNG from the flags as a side effect.
        string hash = GetHash();
        UpdateSeedInfoStr(hash);
        UpdateGoalInfoStr();
        SaveTitleInfo(hash);
        AddRuntimeTextOverrides();
        SaveTextOverrides();

        TextLicenses.Save($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_061.bin.dir\\section_003.bin");
        TextLicenseAugmentDesc.Save($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_061.bin.dir\\section_010.bin");

        TextMenuMessage.Save($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\menu_message.bin");
        TextMenuCommand.Save($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\menu_command.bin");
        TextAbilityHelp.Save($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\listhelp_ability.bin");
        TextKeyDescriptions.Save($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\listhelp_daijinamono.bin");

        foreach (string ebp in Directory.GetFiles($"{Generator.DataOutFolder}\\plan_master\\us\\plan_map", "*.ebp", SearchOption.AllDirectories))
        {
            string id = Path.GetFileNameWithoutExtension(ebp);
            if (TextEbpZones.ContainsKey(id))
            {
                TextEbpZones[id].Save(ebp);
            }
        }
    }

    protected virtual void UpdateGoalInfoStr()
    {
        DataStoreBinText.StringData goalInfoStr = TextEbpZones["rbn_a16"].Values.First(v => v.Text != null && v.Text.Contains("$INFO$"));
        string goalStr = "";
        if (FF12Flags.Items.WritGoals.SelectedValues.Contains(FF12Flags.Items.WritGoalCid2))
        {
            goalStr += "\n" +
                "  -Gather the necessary items to climb the Pharos and defeat {color:gold}Cid 2{rgb:gray}.";
        }

        if (FF12Flags.Items.WritGoals.SelectedValues.Contains(FF12Flags.Items.WritGoalAny))
        {
            goalStr += "\n" +
                "  -Find randomly in a non-missable location. This can be virtually anywhere.";
        }

        if (FF12Flags.Items.WritGoals.SelectedValues.Contains(FF12Flags.Items.WritGoalMaxSphere))
        {
            goalStr += "\n" +
                "  -Find in a random max sphere location (OoA is excluded).\n" +
                "     This requires completing a large portion of the side content.";
        }

        string notesStr = "";
        if (FF12Flags.Items.KeyStartingInv.Enabled)
        {
            notesStr += "\n" +
                "  -Main party member starting items have been randomized.\n" +
                "     Check your inventory for new items after they join.";
        }

        if (!string.IsNullOrEmpty(notesStr))
        {
            goalStr += "{wait}\n" +
                "Other notes for this seed:" +
                notesStr;
        }

        goalInfoStr.Text = goalInfoStr.Text.Replace("$INFO$", "The goal for this seed:\n" +
            "Find a {color:gold}Writ of Transit{rgb:gray} and travel to {italic}Bahamut{/italic}.{wait}\n" +
            "The possible {color:gold}Writ of Transit{rgb:gray} locations for this seed are:" +
            goalStr);
    }

    private void UpdateSeedInfoStr(string hash)
    {
        DataStoreBinText.StringData seedInfoStr = TextEbpZones["rbn_a16"].Values.First(v => v.Text != null && v.Text.Contains("$VERSION$"));
        seedInfoStr.Text = seedInfoStr.Text.Replace("$VERSION$", SetupData.Version);
        seedInfoStr.Text = seedInfoStr.Text.Replace("$SEED$", SeedLabel);
        seedInfoStr.Text = seedInfoStr.Text.Replace("$SEED HASH$", hash);
    }

    private void SaveTitleInfo(string hash)
    {
        File.WriteAllText($"{Generator.OutFolder}\\titleInfo.txt",
            $"FF12 Randomizer v{SetupData.Version}   Seed: {SeedLabel}   {hash}");
    }

    /// <summary>Runs when saving, after every other randomizer has finished.</summary>
    protected virtual void AddRuntimeTextOverrides()
    {
    }

    // Always written, so a seed without overrides never inherits a previous seed's file.
    private void SaveTextOverrides()
    {
        Dictionary<string, string> mapIDs = Generator.Get<TreasureRando>().areaMapping;
        List<string> lines = new()
        {
            "# Runtime text overrides read by RandoText.lua. Generated by the randomizer."
        };

        foreach (var entry in DialogOverrides.OrderBy(e => e.Key.Map).ThenBy(e => e.Key.Index))
        {
            if (!mapIDs.TryGetValue(entry.Key.Map, out string mapID))
            {
                throw new KeyNotFoundException($"Unknown map '{entry.Key.Map}' for a dialog text override.");
            }

            lines.Add($"dialog|{mapID.ToUpperInvariant()}|{entry.Key.Index}|{EscapeOverride(entry.Value)}");
        }

        lines.AddRange(ItemNameOverrides.OrderBy(e => e.Key).Select(e => $"itemname|{e.Key:X4}|{EscapeOverride(e.Value)}"));
        lines.AddRange(ItemMessageOverrides.OrderBy(e => e.Key).Select(e => $"itemmsg|{e.Key:X4}|{EscapeOverride(e.Value)}"));
        lines.AddRange(RewardMessageOverrides.OrderBy(e => e.Key.RewardID).ThenBy(e => e.Key.ItemID ?? -1)
            .Select(e => $"rewardmsg|{e.Key.RewardID:X4}|{(e.Key.ItemID == null ? "gil" : e.Key.ItemID.Value.ToString("X4"))}|{EscapeOverride(e.Value)}"));

        lines.AddRange(RewardNameOverrides.OrderBy(e => e.Key.RewardID).ThenBy(e => e.Key.ItemID)
            .Select(e => $"rewardname|{e.Key.RewardID:X4}|{e.Key.ItemID:X4}|{EscapeOverride(e.Value)}"));

        string ChestMapID(string map)
        {
            return mapIDs.TryGetValue(map, out string mapID)
                ? mapID.ToUpperInvariant()
                : throw new KeyNotFoundException($"Unknown map '{map}' for a chest text override.");
        }

        lines.AddRange(ChestMessageOverrides.OrderBy(e => e.Key.Map).ThenBy(e => e.Key.Respawn).ThenBy(e => e.Key.ItemID)
            .Select(e => $"chestmsg|{ChestMapID(e.Key.Map)}|{e.Key.Respawn}|{e.Key.ItemID:X4}|{EscapeOverride(e.Value)}"));
        lines.AddRange(ChestNameOverrides.OrderBy(e => e.Key.Map).ThenBy(e => e.Key.Respawn).ThenBy(e => e.Key.ItemID)
            .Select(e => $"chestname|{ChestMapID(e.Key.Map)}|{e.Key.Respawn}|{e.Key.ItemID:X4}|{EscapeOverride(e.Value)}"));

        File.WriteAllLines($"{Generator.OutFolder}\\textOverrides.txt", lines);
    }

    private string EscapeOverride(string text)
    {
        // Backslashes first, so a "\n" already in a name stays text and not a line break.
        return text.Replace("\r\n", "\n").Replace("\\", "\\\\").Replace("\n", "\\n");
    }
}
