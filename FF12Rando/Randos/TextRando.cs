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
    public DataStoreBinText TextKeyItems = new();
    public DataStoreBinText TextLoot = new();
    public DataStoreBinText TextMenuMessage = new();
    public DataStoreBinText TextMenuCommand = new();
    public DataStoreBinText TextAbilityHelp = new();
    public DataStoreBinText TextKeyDescriptions = new();

    public Dictionary<string, DataStoreEbpBinText> TextEbpZones = new();
    public TextRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Text Data...");
        TextAbilities.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_061.bin.dir\\section_000.bin");
        TextEquipment.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_061.bin.dir\\section_001.bin");
        TextKeyItems.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_061.bin.dir\\section_012.bin");
        TextLoot.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_061.bin.dir\\section_011.bin");

        TextMenuMessage.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\menu_message.bin");
        TextMenuCommand.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\menu_command.bin");
        TextAbilityHelp.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\listhelp_ability.bin");
        TextKeyDescriptions.Load($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\listhelp_daijinamono.bin");

        List<string> ebpZonesToLoad = new()
        {
            "rbn_a16"
        };

        foreach(string ebp in Directory.GetFiles($"{Generator.DataOutFolder}\\plan_master\\us\\plan_map", "*.ebp", SearchOption.AllDirectories))
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


        DataStoreBinText.StringData seedInfoStr = TextEbpZones["rbn_a16"].Values.First(v => v.Text != null && v.Text.Contains("$VERSION$"));
        seedInfoStr.Text = seedInfoStr.Text.Replace("$VERSION$", SetupData.Version);
        seedInfoStr.Text = seedInfoStr.Text.Replace("$SEED$", RandomNum.GetIntSeed(SetupData.Seed).ToString());
        seedInfoStr.Text = seedInfoStr.Text.Replace("$SEED HASH$", GetHash());

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
}
