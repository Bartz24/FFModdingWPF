using Bartz24.Data;
using Bartz24.FF13Series;
using Bartz24.RandoWPF;
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
        RandoUI.SetUIProgressIndeterminate("Loading Text Data...");
        {
            string path = Nova.GetNovaFile("LR", @"txtres\zone\z0100\txtres_us.ztr", SetupData.Paths["Nova"], SetupData.Paths["LR"]);
            string outPath = Generator.DataOutFolder + @"\txtres\zone\z0100\txtres_us.ztr";
            FileHelpers.CopyFile(path, outPath);

            zone100SysUS.Load("LR", outPath, SetupData.Paths["Nova"]);
        }

        zone100SysUS["$inn_no_time"] = "You usually need an {Color Gold}ID card{Color White}. Open at the cost of all EP?";
        zone100SysUS["$sys_yu_noopn"] = "You need an {Color Gold}ID card{Color White} and have to complete the Warehouse to open the gate.";
        zone100SysUS["$sys_yu_mq2"] = "You need {Color Gold}Serah's Pendant{Color White} and have to check the table.";

        RandoUI.SetUIProgressDeterminate("Loading Text Data...", 50, 100);
        {
            string path = Nova.GetNovaFile("LR", @"txtres\resident\system\txtres_us.ztr", SetupData.Paths["Nova"], SetupData.Paths["LR"]);
            string outPath = Generator.DataOutFolder + @"\txtres\resident\system\txtres_us.ztr";
            FileHelpers.CopyFile(path, outPath);

            mainSysUS.Load("LR", outPath, SetupData.Paths["Nova"]);
        }

        //mainSysUS["$m_001"] = "Rando: Slaughterhouse Special";
        //mainSysUS["$m_001_ac000"] = "Used for tracking in the randomizer. You have checked the Fragment of Courage location in Yusnaan.";

        // Add text for key_r_victory
        if (!mainSysUS.Keys.Contains("key_r_victory"))
        {
            mainSysUS.Add("$zzz_r_victory", "Victory!");
            mainSysUS.Add("$zzz_r_victoryh", "Used for tracking in the randomizer. You have won. Yay. If you see this, hi :)");
        }

        // key_r_ep Add text for Max EP item
        mainSysUS.Add("$zzz_r_ep", "Maximum EP + 1");
        mainSysUS.Add("$zzz_r_eph", "Increases Maximum EP by 1. This should be removed from the inventory automatically.");

        // key_r_atb Add text for Max ATB item
        mainSysUS.Add("$zzz_r_atb", "Maximum ATB + 10");
        mainSysUS.Add("$zzz_r_atbh", "Increases Maximum ATB by 10. This should be removed from the inventory automatically.");

        // key_r_rec Add text for Recovery Item Capacity item
        mainSysUS.Add("$zzz_r_rec", "Recovery Item Capacity + 1");
        mainSysUS.Add("$zzz_r_rech", "Increases the number of Recovery Items you can hold by 1. This should be removed from the inventory automatically.");

        // key_r_dpass Add text for Dead Dunes Train Pass
        mainSysUS.Add("$zzz_r_dpass", "Dead Dunes Train Pass");
        mainSysUS.Add("$zzz_r_dpassh", "Used for unlocking travel to Dead Dunes by train.");

        // key_r_ypass Add text for Yusnaan Train Pass
        mainSysUS.Add("$zzz_r_ypass", "Yusnaan Train Pass");
        mainSysUS.Add("$zzz_r_ypassh", "Used for unlocking travel to Yusnaan by train.");

        // key_r_wpass Add text for Wildlands Train Pass
        mainSysUS.Add("$zzz_r_wpass", "Wildlands Train Pass");
        mainSysUS.Add("$zzz_r_wpassh", "Used for unlocking travel to the Wildlands by train.");

        zone100SysUS.Add("$tra_no_ypass", "{Icon Warning} Yusnaan Train Pass is required to travel to Yusnaan.");
        zone100SysUS.Add("$tra_no_wpass", "{Icon Warning} Wildlands Train Pass is requiredto travel to the Wildlands.");
        zone100SysUS.Add("$tra_no_dpass", "{Icon Warning} Dead Dunes Train Pass is required to travel to the Dead Dunes.");
        zone100SysUS.Add("$wrp_no_egg", "{Icon Warning} The Mystery Egg is required to enter Ultimate Lair.");
    }

    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing Text Data...");
        if (LRFlags.Enemies.EnemyLocations.FlagEnabled)
        {
            const string warn = " (I may crash with {Btn RT})";
            mainSysUS["$m_355"] += warn;
            mainSysUS["$m_455"] += warn;
            mainSysUS["$m_805"] += warn;
            mainSysUS["$m_806w"] += warn;
            mainSysUS["$m_821w"] += warn;
            mainSysUS["$m_896w"] += warn;
        }


        if (LRFlags.Other.LoadingText.FlagEnabled)
        {
            LRFlags.Other.LoadingText.SetRand();
            RandomizeWords(mainSysUS.Keys.Where(k => k.StartsWith("$sns")).ToList());
            RandomNum.ClearRand();
        }

        if (LRFlags.Other.SheepNames.FlagEnabled)
        {
            LRFlags.Other.SheepNames.SetRand();
            RandomizeSheepNames();
            RandomNum.ClearRand();
        }

    }

    private void RandomizeSheepNames()
    {
        List<string> sheepNames = new()
        {
            "Scared Sheep",
            "Sacred Sheep",
            "Fearful Sheep",
            "Shaken Sheep",
            "Stirred Sheep",
            "Sheepish Sheep",
            "Frightened Sheep",
            "Startled Sheep",
            "Panicked Sheep",
            "Terrified Sheep",
            "Anxious Sheep",
            "Worried Sheep",
            "Nervous Sheep",
            "Agitated Sheep",
            "Jittery Sheep",
            "Sleepy Sheep",
            "Angry Sheep",
            "Hopeful Sheep",
            "Commando Sheep",
            "Ravager Sheep",
            "Sentinel Sheep",
            "Synergist Sheep",
            "Saboteur Sheep",
            "Medic Sheep",
            "Not Dr. Sheep",
            "Secret Sheep",
            "Redacted Sheep",
            "Savior Sheep",
            "Vanille's Popped Sheep",
            "Hope",
            "Sheep 2.0",
            "Sheepy McSheepface",
            "Sheep Returns",
            "Final Fantasy XIII",
            "The Story So Far",
            "Kupo?"
        };

        zone100SysUS["$name_qst_1503"] = RandomNum.SelectRandom(sheepNames);
        sheepNames.Remove(zone100SysUS["$name_qst_1503"]);
        zone100SysUS["$name_qst_1504"] = RandomNum.SelectRandom(sheepNames);
        sheepNames.Remove(zone100SysUS["$name_qst_1504"]);
        zone100SysUS["$name_qst_1505"] = RandomNum.SelectRandom(sheepNames);
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
                    iconForm += "{Icon Knife}";
                    break;
                case '1':
                    iconForm += "{Icon Brooch}";
                    break;
                case '2':
                    iconForm += "{Icon Ring}";
                    break;
                case '3':
                    iconForm += "{Icon Hammer}";
                    break;
                case '4':
                    iconForm += "{Icon Spear}";
                    break;
                case '5':
                    iconForm += "{Icon Sword}";
                    break;
                case '6':
                    iconForm += "{Icon Greatsword}";
                    break;
                case '7':
                    iconForm += "{Icon Rapier}";
                    break;
                case '8':
                    iconForm += "{Icon Dual_Blades}";
                    break;
                case '9':
                    iconForm += "{Icon Staff}";
                    break;
            }
        }

        return iconForm;
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Text Data...");
        string hash = GetHash();

        mainSysUS["$dif_conf_e"] = "{Icon Warning} You have selected {Color LightRed}EASY MODE{Color White}.{Text NewLine}" +
            "{Text NewLine}" +
            "{UnkFF D0}Battle Difficulty: Easy{Text NewLine}" +
            "{UnkFF D0}Fleeing battle: No penalty{Text NewLine}" +
            "{UnkFF D0}HP (health): Auto-recovery in field{Text NewLine}" +
            "{UnkFF D0}Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
            "{UnkFF D0}Seed Hash (for validation): " + hash + "{Text NewLine}" +
            "{Text NewLine}" +
            "Game difficulty cannot be changed once the game has started.{Text NewLine}" +
            "{StraightLine}Do you want to continue?|Yes|No";
        mainSysUS["$dif_conf_n"] = "{Icon Warning} You have selected {Color LightRed}NORMAL MODE{Color White}.{Text NewLine}" +
            "{Text NewLine}" +
            "{UnkFF D0}Battle Difficulty: Normal{Text NewLine}" +
            "{UnkFF D0}Fleeing battle: Penalty imposed{Text NewLine}" +
            "{UnkFF D0}HP (health): No auto-recovery in field{Text NewLine}" +
            "{UnkFF D0}Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
            "{UnkFF D0}Seed Hash (for validation): " + hash + "{Text NewLine}" +
            "{Text NewLine}" +
            "Game difficulty cannot be changed once the game has started.{Text NewLine}" +
            "{StraightLine}Do you want to continue?|Yes|No";
        mainSysUS["$dif_conf_h"] = "{Icon Warning} You have selected {Color LightRed}HARD MODE{Color White}.{Text NewLine}" +
            "{Text NewLine}" +
            "{UnkFF D0}Battle Difficulty: Hard{Text NewLine}" +
            "{UnkFF D0}Fleeing battle: Penalty imposed{Text NewLine}" +
            "{UnkFF D0}HP (health): No auto-recovery in field{Text NewLine}" +
            "UnkFF D0}Seed (number form): " + RandomNum.GetIntSeed(SetupData.Seed) + "{Text NewLine}" +
            "{UnkFF D0}Seed Hash (for validation): " + hash + "{Text NewLine}" +
            "{Text NewLine}" +
            "Game difficulty cannot be changed once the game has started.{Text NewLine}" +
            "{StraightLine}Do you want to continue?|Yes|No";

        {
            string outPath = Generator.DataOutFolder + @"\txtres\zone\z0100\txtres_us.ztr";
            zone100SysUS.Save("LR", outPath, SetupData.Paths["Nova"]);
        }

        {
            string outPath = Generator.DataOutFolder + @"\txtres\resident\system\txtres_us.ztr";
            mainSysUS.Save("LR", outPath, SetupData.Paths["Nova"]);
        }
    }
}
