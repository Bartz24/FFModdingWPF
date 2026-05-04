using Bartz24.Data;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando;

public class LicenseRando : Randomizer
{
    public DataStoreBPSection<DataStoreLicense> licenses = new();
    public DataStoreBPSection<DataStoreLicense> licensesOrig = new();

    public DataStoreBPSection<DataStoreAugment> augments = new();

    public DataStoreList<DataStoreLicenseIcon> licenseIcons = new();

    public Dictionary<string, LicenseData> licenseData = new();
    public Dictionary<string, AugmentData> augmentData = new();
    public Dictionary<(string name, DataStoreLicenseIcon.Type type, int layer), LicenseIconData> licenseIconData = new();

    public LicenseRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading License Data...");
        licenses = new();
        licenses.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_012.bin"));
        // Update IDs
        for (int i = 0; i < licenses.DataList.Count; i++)
        {
            licenses.DataList[i].IntID = i;
        }

        licensesOrig = new();
        licensesOrig.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_012.bin"));
        // Update IDs
        for (int i = 0; i < licensesOrig.DataList.Count; i++)
        {
            licensesOrig.DataList[i].IntID = i;
        }

        augments = new();
        augments.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_058.bin"));
        // Update IDs
        for (int i = 0; i < augments.DataList.Count; i++)
        {
            augments.DataList[i].IntID = i;
        }

        licenseIcons = new();
        licenseIcons.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\myoshiok\\in\\bin_menu\\license_chip.bin"));

        if (FF12Flags.Licenses.BoardType.SelectedValue == FF12Flags.Licenses.BoardTypeVanilla)
        {
            FileHelpers.ReadCSVFile(@"data\licenses\vanilla\licenses.csv", row =>
            {
                LicenseData l = new(row, Generator);
                licenseData.Add(l.ID, l);
            }, FileHelpers.CSVFileHeader.HasHeader);
        }
        else if (FF12Flags.Licenses.BoardType.SelectedValue == FF12Flags.Licenses.BoardTypeSplit)
        {
            FileHelpers.ReadCSVFile(@"data\licenses\dual\licenses.csv", row =>
            {
                LicenseData l = new(row, Generator);
                licenseData.Add(l.ID, l);
            }, FileHelpers.CSVFileHeader.HasHeader);
        }

        FileHelpers.ReadCSVFile(@"data\augments.csv", row =>
        {
            AugmentData a = new(row);
            augmentData.Add(a.ID, a);
        }, FileHelpers.CSVFileHeader.HasHeader);

        FileHelpers.ReadCSVFile(@"data\licenseicons.csv", row =>
        {
            LicenseIconData l = new(row);
            licenseIconData.Add((l.Name, l.Type, l.Layer), l);
        }, FileHelpers.CSVFileHeader.HasHeader);
    }
    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing License Data...");

        TextRando textRando = Generator.Get<TextRando>();
        PartyRando partyRando = Generator.Get<PartyRando>();

        // Overwrite license data
        licenseData.Values.ForEach(l =>
        {
            DataStoreLicense license = licenses.DataList[l.IntID];
            textRando.TextLicenses[license.NameAddress - 0x1800].Text = l.Name;
            license.Type = l.Type;
            license.LPCost = (byte)l.LPCost;
            if (l.Type != DataStoreLicense.LicenseType.Esper
                && l.Type != DataStoreLicense.LicenseType.Quickening
                && l.Type != DataStoreLicense.LicenseType.SecondBoard)
            {
                license.ContentsStr = l.Contents;

                bool[] unlocks = new bool[8];

                partyRando.CharacterMapping.ForEach(c =>
                {
                    unlocks[partyRando.CharacterMapping.ToList().IndexOf(c)] = l.DefaultCharacters.Contains(c);
                });

                license.DefaultCharUnlock = unlocks;
            }
        });

        // Overwrite augment data
        augments.DataList.ForEach(augment =>
        {
            augment.LicenseDescAddress = (ushort)(0x5000 + augment.IntID);
            augment.EquipDescAddress = (ushort)(13000 + augment.IntID);

            AugmentData a = augmentData.GetValueOrDefault(augment.ID);
            if (a?.Value >= 0)
            {
                augment.Value = (ushort)a.Value;
            }

            string formattedDesc = a == null ? "UNKNOWN" : a.Description.Replace("$n$", augment.Value.ToString());
            textRando.TextLicenseAugmentDesc[augment.LicenseDescAddress - 0x5000].Text = formattedDesc;

            if (!textRando.TextAbilityHelp.Keys.Contains(augment.EquipDescAddress - 13000))
            {
                textRando.TextAbilityHelp.AddNew(DataStoreBinText.StringData.StringType.Dialog);
            }

            textRando.TextAbilityHelp[augment.EquipDescAddress - 13000].Text = "\n" + formattedDesc;
        });

        FF12Flags.Licenses.LicenseBoardType.SetRand();

        SetLicenseIcons();

        RandomNum.ClearRand();
    }

    private void SetLicenseIcons()
    {
        for (int i = 0; i < licenses.DataList.Count; i++)
        {
            DataStoreLicense license = licenses.DataList[i];
            DataStoreLicenseIcon licenseIcon = licenseIcons[i];

            if (license.Type is
                DataStoreLicense.LicenseType.Esper or
                DataStoreLicense.LicenseType.Quickening or
                DataStoreLicense.LicenseType.SecondBoard or
                DataStoreLicense.LicenseType.Unused or
                DataStoreLicense.LicenseType.Unused2 or
                DataStoreLicense.LicenseType.Unused3 or
                DataStoreLicense.LicenseType.Unused4 or
                DataStoreLicense.LicenseType.Unused5 or
                DataStoreLicense.LicenseType.Unused6 or
                DataStoreLicense.LicenseType.Unused7 or
                DataStoreLicense.LicenseType.Unused8 or
                DataStoreLicense.LicenseType.Unused9)
            {
                continue;
            }

            string iconType = GetLicenseIconType(license);

            ApplyAvailableLayers(license, licenseIcon, iconType);
            ApplyObtainedLayers(license, licenseIcon, iconType);
        }
    }

    private bool IsNumberedLicense(string licenseName)
    {
        return int.TryParse(licenseName.Split(" ").Last(), out int _);
    }

    private int GetNumberedLicense(string licenseName)
    {
        return int.Parse(licenseName.Split(" ").Last());
    }

    private void ClearLayers(DataStoreLicenseIcon licenseIcon, DataStoreLicenseIcon.Type type)
    {
        for (int i = 0; i < 4; i++)
        {
            licenseIcon[type][i].TextureGroup = 255;
            licenseIcon[type][i].TextureSection = 255;
            licenseIcon[type][i].Animation = 0;
            licenseIcon[type][i].ClutLink = 0;
            licenseIcon[type][i].TextureLink = 0;
            licenseIcon[type][i].XPos = 0;
            licenseIcon[type][i].YPos = 0;
        }
    }

    private void ApplyAvailableLayers(DataStoreLicense license, DataStoreLicenseIcon licenseIcon, string iconType)
    {
        ClearLayers(licenseIcon, DataStoreLicenseIcon.Type.Available);

        SetLayer(licenseIcon[DataStoreLicenseIcon.Type.Available][0], licenseIconData[("Background", DataStoreLicenseIcon.Type.Available, 0)]);

        List<LicenseIconData> layers = licenseIconData.Values.Where(l => l.Name == iconType && l.Type == DataStoreLicenseIcon.Type.Available).OrderBy(l => l.Layer).ToList();
        foreach (LicenseIconData layer in layers)
        {
            SetLayer(licenseIcon[DataStoreLicenseIcon.Type.Available][layer.Layer], layer);
        }

        ApplyNumberLayers(DataStoreLicenseIcon.Type.Available, license, licenseIcon, layers.Max(l => l.Layer) + 1);
    }

    private void ApplyObtainedLayers(DataStoreLicense license, DataStoreLicenseIcon licenseIcon, string iconType)
    {
        ClearLayers(licenseIcon, DataStoreLicenseIcon.Type.Obtained);

        List<LicenseIconData> layers = licenseIconData.Values.Where(l => l.Name == iconType && l.Type == DataStoreLicenseIcon.Type.Obtained).OrderBy(l => l.Layer).ToList();
        foreach (LicenseIconData layer in layers)
        {
            SetLayer(licenseIcon[DataStoreLicenseIcon.Type.Obtained][layer.Layer], layer);
        }

        ApplyNumberLayers(DataStoreLicenseIcon.Type.Obtained, license, licenseIcon, layers.Max(l => l.Layer) + 1);
    }

    private void ApplyNumberLayers(DataStoreLicenseIcon.Type type, DataStoreLicense license, DataStoreLicenseIcon licenseIcon, int initialNumLayer)
    {
        TextRando textRando = Generator.Get<TextRando>();

        string licenseName = textRando.TextLicenses[license.NameAddress - 0x1800].Text;
        if (IsNumberedLicense(licenseName))
        {
            string numName = "Num" + GetNumberedLicense(licenseName);
            List<LicenseIconData> layers = licenseIconData.Values.Where(l => l.Name == numName && l.Type == type).OrderBy(l => l.Layer).ToList();
            foreach (LicenseIconData layer in layers)
            {
                SetLayer(licenseIcon[type][initialNumLayer + layer.Layer], layer);
            }
        }
    }

    private void SetLayer(DataStoreLicenseIconLayer layer, LicenseIconData iconData)
    {
        layer.TextureGroup = (byte)iconData.TextureGroup;
        layer.TextureSection = (byte)iconData.TextureSection;
        layer.Animation = (ushort)iconData.Animation;
        layer.ClutLink = (byte)iconData.Clut;
        layer.TextureLink = (byte)iconData.TextureEntry;
        layer.XPos = (sbyte)iconData.X;
        layer.YPos = (sbyte)iconData.Y;
    }

    private string GetLicenseIconType(DataStoreLicense license)
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        switch (license.Type)
        {
            case DataStoreLicense.LicenseType.Weapon:
            case DataStoreLicense.LicenseType.Weapon2:
            case DataStoreLicense.LicenseType.Weapon3:
                EquipCategory commonCategory = license.Contents
                    .GroupBy(id => equipRando.equip.EquipDataList.First(e => e.IntID == id).Category)
                    .Shuffle()
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;

                switch (commonCategory)
                {
                    case EquipCategory.Sword:
                        return "Swords";
                    case EquipCategory.Greatsword:
                        return "Greatswords";
                    case EquipCategory.Katana:
                        return "Katanas";
                    case EquipCategory.NinjaSword:
                        return "NinjaSwords";
                    case EquipCategory.Spear:
                        return "Spears";
                    case EquipCategory.Pole:
                        return "Poles";
                    case EquipCategory.Bow:
                        return "Bows";
                    case EquipCategory.Crossbow:
                        return "Crossbows";
                    case EquipCategory.Gun:
                        return "Guns";
                    case EquipCategory.Axe:
                    case EquipCategory.Hammer:
                        return "AxesHammers";
                    case EquipCategory.Dagger:
                        return "Daggers";
                    case EquipCategory.Rod:
                        return "Rods";
                    case EquipCategory.Staff:
                        return "Staves";
                    case EquipCategory.Mace:
                        return "Maces";
                    case EquipCategory.Measure:
                        return "Measures";
                    case EquipCategory.Handbomb:
                        return "Handbombs";
                    default:
                        throw new Exception("Invalid weapon category for icon: " + commonCategory);
                }
            case DataStoreLicense.LicenseType.Armor:
            case DataStoreLicense.LicenseType.LightArmor:
            case DataStoreLicense.LicenseType.MysticArmor:
                // Check Genji equip
                List<ushort> genji = new()
                {
                    0x10D5,
                    0x1115,
                    0x1151,
                    0x1164
                };

                if (license.Contents.Intersect(genji).Count() > license.Contents.Count / 2)
                {
                    return "GenjiArmor";
                }

                DataStoreArmor.ArmorType armorCat = license.Contents
                    .Where(id => equipRando.equip.EquipDataList.First(e => e.IntID == id) is DataStoreArmor)
                    .GroupBy(id => ((DataStoreArmor)equipRando.equip.EquipDataList.First(e => e.IntID == id)).ArmorCategory)
                    .Shuffle()
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;

                switch (armorCat)
                {
                    case DataStoreArmor.ArmorType.LightArmor:
                        return "LightArmor";
                    case DataStoreArmor.ArmorType.HeavyArmor:
                        return "HeavyArmor";
                    case DataStoreArmor.ArmorType.MysticArmor:
                        return "MysticArmor";
                    default:
                        throw new Exception("Invalid armor category for icon: " + armorCat);
                }
            case DataStoreLicense.LicenseType.Shield:
                return "Shields";
            case DataStoreLicense.LicenseType.Accessory:
                return "Accessories";
            case DataStoreLicense.LicenseType.Augment:
            case DataStoreLicense.LicenseType.Augment2:
            case DataStoreLicense.LicenseType.Augment3:
                AugmentData augment = augmentData[license.ContentsStr[0]];

                List<string> numberedAugments = new()
                {
                    "Shield Block",
                    "Channeling",
                    "Swiftness",
                    "Remedy Lore",
                    "Potion Lore",
                    "Phoenix Lore",
                    "Ether Lore",
                    "Battle Lore",
                    "Magick Lore",
                    "HP Lore"
                };
                // First check if the augment name starts with any of the numbered prefixes and return the prefix
                foreach (string prefix in numberedAugments)
                {
                    if (augment.Name.StartsWith(prefix))
                    {
                        return prefix;
                    }
                }

                switch (augment.Name)
                {
                    case "Warmage":
                    case "Martyr":
                    case "Inquisitor":
                    case "Headsman":
                    case "Adrenaline":
                    case "Spellbreaker":
                    case "Focus":
                    case "Serenity":
                    case "Last Stand":
                    case "Spellbound":
                    case "Brawler":
                    case "Pharmacology":
                    case "Toxicology":
                    case "Geomancy":
                    case "Saboteur":
                    case "LP Boost":
                    case "EXP Boost":
                    case "Piercing Magick":
                    case "Spendthrift":
                    case "Sentinel":
                    case "Safety":
                    case "Ignore Evasion":
                    case "Alert":
                    case "Counter":
                    case "Improved Counter":
                    case "Trigger Happy":
                    case "Master Thief":
                    case "Invisible":
                    case "Awareness":
                    case "Magick Deprived":
                    case "Soulbind":
                        return augment.Name;
                    default:
                        throw new Exception("Invalid augment for icon: " + augment.Name);
                }

            case DataStoreLicense.LicenseType.Technick:
                return licenseData[license.ID].Name;
            case DataStoreLicense.LicenseType.Magick:
            case DataStoreLicense.LicenseType.BlackMagick:
            case DataStoreLicense.LicenseType.TimeMagick:
            case DataStoreLicense.LicenseType.GreenMagick:
            case DataStoreLicense.LicenseType.ArcaneMagick:
                List<string> types = new()
                {
                    "White Magick",
                    "Black Magick",
                    "Time Magick",
                    "Green Magick",
                    "Arcane Magick",
                    "Red Magick",
                    "Umbral Magick",
                    "Primal Magick",
                    "Mystic Magick",
                    "Runic Magick",
                    "Elemental Magick",
                };
                // Check if the augment name contains any of the magick types and return the type
                foreach (string type in types)
                {
                    if (licenseData[license.ID].Name.Contains(type))
                    {
                        return type;
                    }
                }

                throw new Exception("Invalid magick type for icon: " + licenseData[license.ID].Name);

            case DataStoreLicense.LicenseType.EssentialGambit:
                if (license.IntID == 31)
                {
                    return "Essentials";
                }
                else if (licenseData[license.ID].Name.Contains("Gambit Slot"))
                {
                    return "Gambits";
                }
                else
                {
                    throw new Exception("Invalid essential gambit for icon: " + licenseData[license.ID].Name);
                }
            default:
                throw new Exception("Invalid license type for icon: " + license.Type);
        }
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving License Data...");
        File.WriteAllBytes($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_012.bin", licenses.Data);
        File.WriteAllBytes($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_058.bin", augments.Data);
        File.WriteAllBytes($"{Generator.DataOutFolder}\\image\\ff12\\myoshiok\\in\\bin_menu\\license_chip.bin", licenseIcons.Data);
    }

    public class LicenseData : CSVDataRow
    {
        [RowIndex(0)]
        public int IntID { get; set; }
        public string ID { get; set; }
        [RowIndex(1)]
        public string Name { get; set; }
        [RowIndex(2)]
        public DataStoreLicense.LicenseType Type { get; set; }
        [RowIndex(4)]
        public int LPCost { get; set; }
        [RowIndex(5)]
        public List<string> DefaultCharacters { get; set; }
        public List<string> Contents
        {
            get => new List<string>
            {
                Contents1,
                Contents2,
                Contents3,
                Contents4,
                Contents5,
                Contents6,
                Contents7,
                Contents8
            }
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(s => GetContentID(s)).ToList();
        }
        [RowIndex(6)]
        public string Contents1 { get; set; }
        [RowIndex(7)]
        public string Contents2 { get; set; }
        [RowIndex(8)]
        public string Contents3 { get; set; }
        [RowIndex(9)]
        public string Contents4 { get; set; }
        [RowIndex(10)]
        public string Contents5 { get; set; }
        [RowIndex(11)]
        public string Contents6 { get; set; }
        [RowIndex(12)]
        public string Contents7 { get; set; }
        [RowIndex(13)]
        public string Contents8 { get; set; }

        private SeedGenerator Generator { get; set; }
        public LicenseData(string[] row, SeedGenerator generator) : base(row)
        {
            Generator = generator;
            ID = IntID.ToString("X4");
        }

        private string GetContentID(string name)
        {
            TextRando textRando = Generator.Get<TextRando>();
            EquipRando equipRando = Generator.Get<EquipRando>();
            LicenseRando licenseRando = Generator.Get<LicenseRando>();
            int intId = -1;

            switch (Type)
            {
                case DataStoreLicense.LicenseType.Magick:
                case DataStoreLicense.LicenseType.BlackMagick:
                case DataStoreLicense.LicenseType.TimeMagick:
                case DataStoreLicense.LicenseType.GreenMagick:
                case DataStoreLicense.LicenseType.ArcaneMagick:
                    intId = equipRando.itemData.Values.Where(s => s.Name == name).First().IntID - 0x3000;
                    break;
                case DataStoreLicense.LicenseType.Technick:
                    intId = equipRando.itemData.Values.Where(s => s.Name == name).First().IntID - 0x4000 + 158;
                    break;
                case DataStoreLicense.LicenseType.Weapon:
                case DataStoreLicense.LicenseType.Weapon2:
                case DataStoreLicense.LicenseType.Weapon3:
                case DataStoreLicense.LicenseType.Armor:
                case DataStoreLicense.LicenseType.LightArmor:
                case DataStoreLicense.LicenseType.MysticArmor:
                case DataStoreLicense.LicenseType.Shield:
                case DataStoreLicense.LicenseType.Accessory:
                    intId = equipRando.itemData.Values.Where(s => s.Name == name).First().IntID;
                    break;
                case DataStoreLicense.LicenseType.Augment:
                case DataStoreLicense.LicenseType.Augment2:
                case DataStoreLicense.LicenseType.Augment3:
                case DataStoreLicense.LicenseType.EssentialGambit:
                    intId = licenseRando.augmentData.Values.Where(a => a.Name == name).First().IntID;
                    break;
                default:
                    throw new Exception("Invalid license type for content ID mapping: " + Type);
            }

            return intId.ToString("X4");
        }
    }

    public class LicenseIconData : CSVDataRow
    {
        [RowIndex(0)]
        public string Name { get; set; }
        [RowIndex(1)]
        public DataStoreLicenseIcon.Type Type { get; set; }
        [RowIndex(2)]
        public int Layer { get; set; }
        [RowIndex(3)]
        public int TextureGroup { get; set; }
        [RowIndex(4)]
        public int TextureSection { get; set; }
        [RowIndex(5)]
        public int Animation { get; set; }
        [RowIndex(6)]
        public int Clut { get; set; }
        [RowIndex(7)]
        public int TextureEntry { get; set; }
        [RowIndex(8)]
        public int X { get; set; }
        [RowIndex(9)]
        public int Y { get; set; }
        public LicenseIconData(string[] row) : base(row)
        {
        }
    }
}
