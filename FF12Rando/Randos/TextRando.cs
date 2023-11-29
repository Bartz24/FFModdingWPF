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
        Generator.SetUIProgress("Loading Text Data...", 0, -1);
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
        Generator.SetUIProgress("Randomizing Text Data...", 0, -1);
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
        Generator.SetUIProgress("Saving Text Data...", 0, -1);


        DataStoreBinText.StringData infoStr = TextEbpZones["rbn_a16"].Values.First(v => v.Text != null && v.Text.Contains("$VERSION$"));
        infoStr.Text = infoStr.Text.Replace("$VERSION$", SetupData.Version);
        infoStr.Text = infoStr.Text.Replace("$SEED$", RandomNum.GetIntSeed(SetupData.Seed).ToString());
        infoStr.Text = infoStr.Text.Replace("$SEED HASH$", GetHash());

        string notesStr = "";
        if (!FF12Flags.Items.KeyWrit.Enabled)
        {
            notesStr += "\n" +
                "-The {color:gold}Writ of Transit{rgb:gray} is not a possible goal this seed.";
        }

        if (FF12Flags.Items.KeyStartingInv.Enabled)
        {
            notesStr += "\n" +
                "-Main party member starting items have been randomized.\n" +
                "\tCheck your inventory for new items after they join.";
        }

        if (!string.IsNullOrEmpty(notesStr))
        {
            infoStr.Text += "{wait}\n" +
                "Notes for this seed:" +
                notesStr;
        }

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
