using Bartz24.Data;
using Bartz24.FF13_2;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FF13_2Rando;

public class EquipRando : Randomizer
{
    public DataStoreWDB<DataStoreItemWeapon> itemWeapons = new();
    public DataStoreWDB<DataStoreItem> items = new();
    public readonly Dictionary<string, ItemData> itemData = new();

    public EquipRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Item/Equip Data...");
        itemWeapons.LoadWDB(Generator, "13-2", @"\db\resident\item_weapon.wdb");
        FileHelpers.CopyFile(Generator.DataOutFolder + @"\db\resident\item_weapon.wdb", Generator.DataOutFolder + @"\db\resident\item_weapon.wdb.orig");
        items.LoadWDB(Generator, "13-2", @"\db\resident\item.wdb");

        FileHelpers.ReadCSVFile(@"data\items.csv", row =>
        {
            ItemData i = new(row);
            itemData.Add(i.ID, i);
        }, FileHelpers.CSVFileHeader.HasHeader);

        items.Copy("key_l_knife", "key_mog_level");
        items["key_mog_level"].sItemNameStringId = "$mog_level";
        items["key_mog_level"].sHelpStringId = "$mog_levelh";

        items.Copy("key_l_knife", "key_shop_level");
        items["key_shop_level"].sItemNameStringId = "$shop_level";
        items["key_shop_level"].sHelpStringId = "$shop_levelh";

        items.Copy("key_l_knife", "key_r_victory");
        items["key_r_victory"].sItemNameStringId = "$victory";
        items["key_r_victory"].sHelpStringId = "$victory";

        string[] fragmentSkillKeys = ["01", "02", "03", "04", "05", "06", "08", "10", "11", "12", "14", "15", "18"];

        foreach (var key in fragmentSkillKeys)
        {
            var itemName = $"privilege{key}";
            items.Copy("key_l_knife", itemName);
            items[itemName].sItemNameStringId = "$"+itemName;
            items[itemName].sHelpStringId = "$"+itemName+"h";
        }

        // Add fake items here for artefacts
        string[] fakeArtefactIds = ["opt_bjaa03_bj", "opt_ddha01_bj", "opt_bjba01_gy", "opt_ghaa01_gt", "opt_gdaa01_vp", "opt_acea02_gy", "opt_gyba01_sn", "opt_gtca02_gw", "opt_gwda01_gw", "opt_aaea03_vp"];
        foreach(string s in fakeArtefactIds)
        {
            items.Copy("key_l_knife", s);
            items[s].sItemNameStringId = $"${s}";
            items[s].sHelpStringId = $"${s}h";
            items[s].u8MenuIcon = 7;
        }
    }
    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Item/Equip Data...");
        if (FF13_2Flags.Stats.EquipStats.FlagEnabled)
        {
            FF13_2Flags.Stats.EquipStats.SetRand();
            RandomizeStats();
            RandomNum.ClearRand();
        }

        if (FF13_2Flags.Stats.EquipPassives.FlagEnabled)
        {
            FF13_2Flags.Stats.EquipPassives.SetRand();
            RandomizePassives();
            RandomNum.ClearRand();
        }

        if (FF13_2Flags.Stats.EquipWeights.FlagEnabled)
        {
            FF13_2Flags.Stats.EquipWeights.SetRand();
            RandomizeWeights();
            RandomNum.ClearRand();
        }
    }

    private void RandomizeStats()
    {
        foreach (DataStoreItemWeapon weapon in itemWeapons.Values.Where(w => w.record.Contains("wea")))
        {
            StatPoints statPoints;
            (int, int)[] bounds = {
                (1, 300),
                (1, 300)
            };
            float[] weights = { 1, 1 };
            int[] chances = { 1, 1 };
            int[] zeros = { 0, 0 };
            int[] negs = { 0, 0 };
            statPoints = new StatPoints(bounds, weights, chances, zeros, negs);
            statPoints.Randomize(new int[] { weapon.i16AttackModVal, weapon.i16MagicModVal });

            weapon.i16AttackModVal = statPoints[0];
            weapon.i16MagicModVal = statPoints[1];

#if DEBUG
            /*weapon.i16AttackModVal = 10000;
            weapon.i16MagicModVal = 10000;
            weapon.i16HpModVal = 30000;
            weapon.i16AtbSpeedModVal = 100;*/
#endif

        }
    }

    private void RandomizePassives()
    {
        CrystariumRando crystariumRando = Generator.Get<CrystariumRando>();
        List<AbilityData> filteredAbilities = crystariumRando.abilityData.Values.Where(a => a.Role == "" && !a.Traits.Contains("Mon")).ToList();
        foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(w => w.sAbility != ""))
        {
            IList<AbilityData> list = filteredAbilities.Where(a => (!a.Traits.Contains("Noel") || equip.record.Contains("noe")) && (!a.Traits.Contains("Serah") || equip.record.Contains("ser"))).Shuffle();
            equip.sAbility = list.First().ID;
        }

        foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(w => w.sAbility2 != ""))
        {
            IList<AbilityData> list = filteredAbilities.Where(a => a.ID != equip.sAbility && (!a.Traits.Contains("Noel") || equip.record.Contains("noe")) && (!a.Traits.Contains("Serah") || equip.record.Contains("ser"))).Shuffle();
            equip.sAbility2 = list.First().ID;
        }

        foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(w => w.sAbility3 != ""))
        {
            IList<AbilityData> list = filteredAbilities.Where(a => a.ID != equip.sAbility && a.ID != equip.sAbility2 && (!a.Traits.Contains("Noel") || equip.record.Contains("noe")) && (!a.Traits.Contains("Serah") || equip.record.Contains("ser"))).Shuffle();
            equip.sAbility3 = list.First().ID;
        }
    }

    private void RandomizeWeights()
    {
        foreach (DataStoreItemWeapon equip in itemWeapons.Values.Where(e => e.u7Cost > 0))
        {
            int range = FF13_2Flags.Stats.WeightRange.Value;
            equip.u7Cost = RandomNum.RandInt(Math.Max(1, equip.u7Cost - range), Math.Min(100, equip.u7Cost + range));
        }
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Item/Equip Data...");
        items.SaveWDB(Generator, @"\db\resident\item.wdb");
        itemWeapons.SaveWDB(Generator, @"\db\resident\item_weapon.wdb");
    }

    public string GetItemName(string itemID)
    {
        TextRando textRando = Generator.Get<TextRando>();
        string name;
        if(itemID == "")
        {
            name = "Gil";
        }
        if (items.Keys.Contains(itemID) && textRando.mainSysUS.Keys.Contains(items[itemID].sItemNameStringId))
        {
            name = textRando.mainSysUS[items[itemID].sItemNameStringId];
            if (name.Contains("{End}"))
            {
                name = name.Substring(0, name.IndexOf("{End}"));
            }
        }
        else
        {
            name = itemID;
        }

        return name;
    }
}
