using Bartz24.Data;
using Bartz24.FF13Series;
using Bartz24.RandoWPF;
using LRRando;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LRRando;

public class TextRando : Randomizer
{
    public DataStoreZTRText zone100SysUS = new();
    public DataStoreZTRText mainSysUS = new();

    public TextRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        Generator.SetUIProgress("Loading Text Data...", 0, 100);
        {
            string path = Nova.GetNovaFile("LR", @"txtres\zone\z0100\txtres_us.ztr", SetupData.Paths["Nova"], SetupData.Paths["LR"]);
            string outPath = Generator.DataOutFolder + @"\txtres\zone\z0100\txtres_us.ztr";
            FileHelpers.CopyFile(path, outPath);

            zone100SysUS.Load(outPath, SetupData.Paths["Nova"]);
        }

        zone100SysUS["$inn_no_time"] = "You usually need an {Color Gold}ID card{Color SkyBlue}. Open at the cost of all EP?";
        zone100SysUS["$sys_yu_noopn"] = "You need an {Color Gold}ID card{Color SkyBlue} and have to complete the Warehouse to open the gate.";
        zone100SysUS["$sys_yu_mq2"] = "You need {Color Gold}Serah's Pendant{Color SkyBlue} and have to check the table.";

        Generator.SetUIProgress("Loading Text Data...", 50, 100);
        {
            string path = Nova.GetNovaFile("LR", @"txtres\resident\system\txtres_us.ztr", SetupData.Paths["Nova"], SetupData.Paths["LR"]);
            string outPath = Generator.DataOutFolder + @"\txtres\resident\system\txtres_us.ztr";
            FileHelpers.CopyFile(path, outPath);

            mainSysUS.Load(outPath, SetupData.Paths["Nova"]);
        }

        //mainSysUS["$m_001"] = "Rando: Slaughterhouse Special";
        //mainSysUS["$m_001_ac000"] = "Used for tracking in the randomizer. You have checked the Fragment of Courage location in Yusnaan.";
        mainSysUS["$m_001_ac100"] = "Rando: Multiworld Dummy";
        mainSysUS["$m_002"] = "Used to tell the multiworld program that the item has been added.";

    }
    public override void Randomize()
    {
        Generator.SetUIProgress("Randomizing Text Data...", -1, 100);
        if (LRFlags.Enemies.EnemyLocations.FlagEnabled)
        {
            mainSysUS["$m_355"] += " (I may crash with {Key R2})";
            mainSysUS["$m_455"] += " (I may crash with {Key R2})";
            mainSysUS["$m_805"] += " (I may crash with {Key R2})";
            mainSysUS["$m_806w"] += " (I may crash with {Key R2})";
            mainSysUS["$m_821w"] += " (I may crash with {Key R2})";
            mainSysUS["$m_896w"] += " (I may crash with {Key R2})";
        }

        LRFlags.Other.HintsPilgrim.SetRand();

        //EquipRando equipRando = randomizers.Get<EquipRando>();
        if (LRFlags.Other.LoadingText.FlagEnabled)
        {
            RandomizeWords(mainSysUS.Keys.Where(k => k.StartsWith("$sns")).ToList());
        }

        RandomNum.ClearRand();
    }

    private void RandomizeWords(List<string> validKeys)
    {
        Dictionary<string, int> wordDictionary = GetWords(validKeys);
        List<DataStoreZTRText> ztrs = new()
        {
            mainSysUS,
            zone100SysUS
        };

        ztrs.ForEach(ztr =>
        {
            ztr.Keys.Where(k => validKeys.Contains(k)).ForEach(k =>
            {
                string[] parts = SplitString(ztr[k]);
                for (int i = 0; i < parts.Length; i++)
                {
                    string word = parts[i];
                    if (!Punctuation.Contains(word.ToLower()) && !string.IsNullOrWhiteSpace(word) && (!word.StartsWith("{") || word.StartsWith("{Key")) && !int.TryParse(word, out _) && !IgnoredWords.Contains(word.ToLower()))
                    {
                        string next = RandomNum.SelectRandomWeighted(wordDictionary.Keys.ToList(), s => wordDictionary[s]);
                        string modified = next;

                        if (word.Length > 1 && word[0].ToString().ToUpper() == word[0].ToString())
                        {
                            modified = modified[0].ToString().ToUpper() + modified.Substring(1);
                        }

                        parts[i] = modified;

                        wordDictionary[next]--;
                        if (wordDictionary[next] == 0)
                        {
                            wordDictionary.Remove(next);
                        }
                    }
                }

                ztr[k] = string.Join("", parts);
            });
        });
    }

    private Dictionary<string, int> GetWords(List<string> validKeys)
    {
        Dictionary<string, int> dict = new();
        List<DataStoreZTRText> ztrs = new()
        {
            mainSysUS,
            zone100SysUS
        };

        ztrs.ForEach(ztr =>
        {
            ztr.Keys.Where(k => validKeys.Contains(k)).ForEach(k =>
            {
                string[] parts = SplitString(ztr[k]);
                foreach (string w in parts.Where(w => !Punctuation.Contains(w.ToLower()) && !string.IsNullOrWhiteSpace(w) && !IgnoredWords.Contains(w.ToLower())))
                {
                    string add = w.ToLower();
                    if (w.StartsWith("{"))
                    {
                        if (!w.StartsWith("{Key"))
                        {
                            continue;
                        }

                        add = w;
                    }

                    if (int.TryParse(w, out int _))
                    {
                        continue;
                    }

                    if (dict.ContainsKey(add))
                    {
                        dict[add] += 1;
                    }
                    else
                    {
                        dict.Add(add, 1);
                    }
                }
            });
        });

        return dict;
    }

    private string[] SplitString(string value)
    {
        List<string> parts = new();
        bool foundPunc = true;
        while (foundPunc)
        {
            foundPunc = false;
            string leftmostPunc = "";
            int leftmostPuncIndex = -1;
            foreach (string p in Punctuation)
            {
                int puncIndex = value.IndexOf(p);
                if (puncIndex != -1 && (leftmostPuncIndex == -1 || puncIndex < leftmostPuncIndex))
                {
                    leftmostPunc = p;
                    leftmostPuncIndex = puncIndex;
                    foundPunc = true;
                }
            }

            if (foundPunc)
            {
                if (leftmostPuncIndex > 0)
                {
                    parts.Add(value.Substring(0, leftmostPuncIndex));
                }

                if (leftmostPunc == "{")
                {
                    parts.Add(value.Substring(leftmostPuncIndex, value.IndexOf("}") - leftmostPuncIndex + 1));
                    value = value.Substring(value.IndexOf("}") + 1);
                }
                else
                {
                    parts.Add(leftmostPunc);
                    value = value.Substring(leftmostPuncIndex + leftmostPunc.Length);
                }
            }
            else
            {
                parts.Add(value);
            }
        }

        return parts.ToArray();
    }

    private string[] Punctuation { get; set; } = { " ", ".", ",", "{Text NewLine}", "!", "{", "(", ")", "?", ":", "-", "+" };
    private string[] IgnoredWords { get; set; } = { "of", "the", "at", "a", "in", "on", "its", "an", "i", "i'm" };

    private string GetHash()
    {
        string numberForm = RandomNum.GetHash(6);
        string iconForm = "";

        foreach (char c in numberForm)
        {
            switch (c)
            {
                case '0':
                    iconForm += "{Icon Shotgun}";
                    break;
                case '1':
                    iconForm += "{Icon Wrench}";
                    break;
                case '2':
                    iconForm += "{Icon Eye01}";
                    break;
                case '3':
                    iconForm += "{Icon Doc}";
                    break;
                case '4':
                    iconForm += "{Icon Potion03}";
                    break;
                case '5':
                    iconForm += "{Icon Feather}";
                    break;
                case '6':
                    iconForm += "{Icon Boomerang}";
                    break;
                case '7':
                    iconForm += "{Icon Gunblade}";
                    break;
                case '8':
                    iconForm += "{Icon Weapon03}";
                    break;
                case '9':
                    iconForm += "{Icon Clock}";
                    break;
            }
        }

        return iconForm;
    }

    public override void Save()
    {
        Generator.SetUIProgress("Saving Text Data...", -1, 100);
        string hash = GetHash();

        mainSysUS["$dif_conf_e"] = "{Icon Attention} You have selected {Color Red}EASY MODE{Color SkyBlue}.{Text NewLine}" +
            "{Text NewLine}" +
            "{VarFF 208}Battle Difficulty: Easy{Text NewLine}" +
            "{VarFF 208}Fleeing battle: No penalty{Text NewLine}" +
            "{VarFF 208}HP (health): Auto-recovery in field{Text NewLine}" +
            "{VarFF 208}Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
            "{VarFF 208}Seed Hash (for validation): " + hash + "{Text NewLine}" +
            "{Text NewLine}" +
            "Game difficulty cannot be changed once the game has started.{Text NewLine}" +
            "{Many}Do you want to continue?|Yes|No";
        mainSysUS["$dif_conf_n"] = "{Icon Attention} You have selected {Color Red}NORMAL MODE{Color SkyBlue}.{Text NewLine}" +
            "{Text NewLine}" +
            "{VarFF 208}Battle Difficulty: Normal{Text NewLine}" +
            "{VarFF 208}Fleeing battle: Penalty imposed{Text NewLine}" +
            "{VarFF 208}HP (health): No auto-recovery in field{Text NewLine}" +
            "{VarFF 208}Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
            "{VarFF 208}Seed Hash (for validation): " + hash + "{Text NewLine}" +
            "{Text NewLine}" +
            "Game difficulty cannot be changed once the game has started.{Text NewLine}" +
            "{Many}Do you want to continue?|Yes|No";
        mainSysUS["$dif_conf_h"] = "{Icon Attention} You have selected {Color Red}HARD MODE{Color SkyBlue}.{Text NewLine}" +
            "{Text NewLine}" +
            "{VarFF 208}Battle Difficulty: Hard{Text NewLine}" +
            "{VarFF 208}Fleeing battle: Penalty imposed{Text NewLine}" +
            "{VarFF 208}HP (health): No auto-recovery in field{Text NewLine}" +
            "{VarFF 208}Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
            "{VarFF 208}Seed Hash (for validation): " + hash + "{Text NewLine}" +
            "{Text NewLine}" +
            "Game difficulty cannot be changed once the game has started.{Text NewLine}" +
            "{Many}Do you want to continue?|Yes|No";

        TempTextCleanup(zone100SysUS);
        TempTextCleanup(mainSysUS);

        {
            string outPath = Generator.DataOutFolder + @"\txtres\zone\z0100\txtres_us.ztr";
            zone100SysUS.Save("LR", outPath, SetupData.Paths["Nova"]);
        }

        {
            string outPath = Generator.DataOutFolder + @"\txtres\resident\system\txtres_us.ztr";
            mainSysUS.Save("LR", outPath, SetupData.Paths["Nova"]);
        }
    }

    private void TempTextCleanup(DataStoreZTRText text)
    {
        text.Keys.ForEach(k =>
        {
            text[k] = text[k].Replace("Ⅷ", "");
            text[k] = text[k].Replace("×", "x");
            text[k] = text[k].Replace("{VarF5 SkyBlue}", "Soul Seeds");
            text[k] = text[k].Replace("{VarF2 PurpleDark}", "  ");
        });
    }
}
