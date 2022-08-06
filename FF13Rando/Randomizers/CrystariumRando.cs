using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF13Rando
{
    public class CrystariumRando : Randomizer
    {
        public Dictionary<string, DataStoreWDB<DataStoreCrystarium>> crystariums = new Dictionary<string, DataStoreWDB<DataStoreCrystarium>>();
        public Dictionary<string, float[]> charMults = new Dictionary<string, float[]>();
        public Dictionary<string, Role[]> primaryRoles = new Dictionary<string, Role[]>();

        public Dictionary<string, AbilityData> abilityData = new Dictionary<string, AbilityData>();

        private string[] chars = new string[] { "lightning", "fang", "hope", "sazh", "snow", "vanille" };
        public Dictionary<string, Dictionary<Role, DataStoreCrystarium>> firstAbis = new Dictionary<string, Dictionary<Role, DataStoreCrystarium>>();

        public CrystariumRando(RandomizerManager randomizers) : base(randomizers) { }

        public override string GetID()
        {
            return "Crystarium";
        }

        public override void Load()
        {
            Randomizers.SetProgressFunc("Loading Crystarium Data...", 0, 100);
            primaryRoles.Add("lightning", new Role[] { Role.Commando, Role.Ravager, Role.Medic });
            primaryRoles.Add("fang", new Role[] { Role.Commando, Role.Sentinel, Role.Saboteur });
            primaryRoles.Add("snow", new Role[] { Role.Commando, Role.Ravager, Role.Sentinel });
            primaryRoles.Add("sazh", new Role[] { Role.Commando, Role.Ravager, Role.Synergist });
            primaryRoles.Add("hope", new Role[] { Role.Ravager, Role.Synergist, Role.Medic });
            primaryRoles.Add("vanille", new Role[] { Role.Ravager, Role.Saboteur, Role.Medic });


            crystariums = chars.ToDictionary(c => c, c => new DataStoreWDB<DataStoreCrystarium>());
            chars.ForEach(c => crystariums[c].LoadWDB("13", @"\db\crystal\crystal_" + c + ".wdb"));

            Randomizers.SetProgressFunc("Loading Crystarium Data...", 80, 100);
            FileHelpers.ReadCSVFile(@"data\abilities.csv", row =>
            {
                AbilityData a = new AbilityData(row);
                abilityData.Add(a.ID, a);
            }, FileHelpers.CSVFileHeader.HasHeader);
        }
        public override void Randomize(Action<int> progressSetter)
        {
            Randomizers.SetProgressFunc("Randomizing Crystarium Data...", 0, 100);
            List<int[]> averageStats = GetAverageStats();
            GetFirstAbilities();
            UpdateCPCosts();

            Randomizers.SetProgressFunc("Randomizing Crystarium Data...", 10, 100);
            if (FF13Flags.Stats.RandCrystAbi.FlagEnabled)
            {
                FF13Flags.Stats.RandCrystAbi.SetRand();
                RandomizeAbilities();
                RandomNum.ClearRand();
            }

            Randomizers.SetProgressFunc("Randomizing Crystarium Data...", 20, 100);
            if (FF13Flags.Stats.RandCrystStat.FlagEnabled)
            {
                FF13Flags.Stats.RandCrystStat.SetRand();
                RandomizeStatValues();
                RandomNum.ClearRand();
            }

            Randomizers.SetProgressFunc("Randomizing Crystarium Data...", 40, 100);
            if (FF13Flags.Stats.ShuffleCrystMisc.Enabled)
            {
                FF13Flags.Stats.RandInitStats.SetRand();
                ShuffleNodesBetweenRoles();
                RandomNum.ClearRand();
            }

            Randomizers.SetProgressFunc("Randomizing Crystarium Data...", 50, 100);
            if (FF13Flags.Stats.ShuffleCrystRole.FlagEnabled)
            {
                FF13Flags.Stats.ShuffleCrystRole.SetRand();
                ShuffleNodes();
                RandomNum.ClearRand();
            }

            Randomizers.SetProgressFunc("Randomizing Crystarium Data...", 80, 100);
            if (FF13Flags.Stats.RandCrystStat.FlagEnabled)
            {
                FF13Flags.Stats.RandCrystStat.SetRand();
                RandomizeCrystariumStats(averageStats);
                RandomNum.ClearRand();
            }

            Randomizers.SetProgressFunc("Randomizing Crystarium Data...", 90, 100);
            if (FF13Flags.Stats.RandInitStats.FlagEnabled)
            {
                FF13Flags.Stats.RandInitStats.SetRand();
                RandomizeInitialStats();
                RandomNum.ClearRand();
            }
        }

        private List<int[]> GetAverageStats()
        {
            List<int[]> list = new List<int[]>();
            for (int stage = 1; stage <= 10; stage++)
            {
                int[] stats = new int[3];
                stats[0] = (int)chars.SelectMany(c => crystariums[c].Values.Where(node => node.iStage == stage && node.iType == CrystariumType.HP && primaryRoles[c].Contains(node.iRole))).Average(node => node.iValue);
                int avgStrMag = (int)chars.SelectMany(c => crystariums[c].Values.Where(node => node.iStage == stage && (node.iType == CrystariumType.Strength || node.iType == CrystariumType.Magic) && primaryRoles[c].Contains(node.iRole))).Average(node => node.iValue);
                stats[1] = avgStrMag;
                stats[2] = avgStrMag;

                list.Add(stats);
            }
            return list;
        }

        private void GetFirstAbilities()
        {
            chars.ForEach(c => firstAbis.Add(c, new Dictionary<Role, DataStoreCrystarium>()));
            foreach (string chara in chars)
            {
                List<DataStoreCrystarium> abilityNodes = crystariums[chara].Values.Where(c => c.iType == CrystariumType.Ability).ToList();

                for (int r = 1; r <= 6; r++)
                {
                    DataStoreCrystarium first = abilityNodes.First(c => c.iRole == (Role)r && abilityData[c.sAbility_string].Role != Role.None);
                    firstAbis[chara].Add((Role)r, first);
                }
            }
        }

        private void RandomizeAbilities()
        {
            foreach (string chara in chars)
            {
                int attempts = 0;
                int maxAttempts = 10000;
                bool success;
                do
                {
                    success = true;
                    attempts++;
                    List<DataStoreCrystarium> abilityNodes = crystariums[chara].Values.Where(c => c.iType == CrystariumType.Ability).ToList();
                    List<DataStoreCrystarium> remaining = new List<DataStoreCrystarium>(abilityNodes);
                    List<string> abilitiesPlaced = new List<string>();

                    // Set the initial abilities
                    for (int r = 1; r <= 6; r++)
                    {
                        DataStoreCrystarium first = firstAbis[chara][(Role)r];
                        string next;
                        do
                        {
                            next = GetNextAbilityRole(abilitiesPlaced, chara, (Role)r, false, false);
                        } while (abilityData[next].Traits.Contains("Required"));

                        first.sAbility_string = next;
                        remaining.Remove(first);
                        abilitiesPlaced.Add(first.sAbility_string);
                    }

                    // Set remaining abilities
                    for (int stage = 1; stage <= 10; stage++)
                    {
                        List<DataStoreCrystarium> nodes = remaining.Where(c => c.iStage == stage).ToList().Shuffle().ToList();

                        foreach (DataStoreCrystarium node in nodes)
                        {
                            IEnumerable<AbilityData> required = abilityData.Values.Where(a => a.Characters.Contains(chara) && a.Traits.Contains("Required") && !abilitiesPlaced.Contains(a.ID));
                            if (remaining.Count == required.Count())
                            {
                                node.sAbility_string = required.ToList().Shuffle().Select(a => a.ID).First();
                                remaining.Remove(node);
                                abilitiesPlaced.Add(node.sAbility_string);
                                continue;
                            }

                            if (node.iCPCost > 0 && FF13Flags.Stats.RandCrystAbiAll.Enabled)
                                node.sAbility_string = GetNextAbilityAll(abilitiesPlaced, chara, true);
                            else
                                node.sAbility_string = GetNextAbilityRole(abilitiesPlaced, chara, node.iRole, true, true);

                            if (node.sAbility_string == null)
                            {
                                success = false;
                                continue;
                            }

                            remaining.Remove(node);
                            abilitiesPlaced.Add(node.sAbility_string);
                        }
                    }
                } while (attempts < maxAttempts && !success);
                if (!success)
                    throw new Exception("Failed to place abilities for " + chara);
            }
        }

        private string GetNextAbilityAll(List<string> used, string chara, bool allowAuto)
        {
            return abilityData.Values.Where(a => !used.Contains(a.ID) && (allowAuto || !a.Traits.Contains("Auto")) && a.Requirements.IsValid(used.ToDictionary(s => s, _ => 1)) && a.Incompatible.Intersect(used).Count() == 0 && a.Characters.Contains(chara))
                .ToList().Shuffle().Select(a => a.ID).FirstOrDefault();
        }
        private string GetNextAbilityRole(List<string> used, string chara, Role role, bool allowTech, bool allowAuto)
        {
            return abilityData.Values.Where(a => !used.Contains(a.ID) && (allowAuto || !a.Traits.Contains("Auto")) && (allowTech && a.Role == Role.None || a.Role == role) && a.Requirements.IsValid(used.ToDictionary(s => s, _ => 1)) && a.Incompatible.Intersect(used).Count() == 0 && a.Characters.Contains(chara))
                .ToList().Shuffle().Select(a => a.ID).FirstOrDefault();
        }

        private void UpdateCPCosts()
        {
            uint[] costs = new uint[] { 60, 90, 220, 230, 400, 1000, 740, 6000, 12000, 45000 };
            UpdateNodeCPCost(GetFirstRole("lig"), "lightning", costs);

            costs = new uint[] { 75, 90, 380, 145, 420, 1000, 970, 6000, 12000, 45000 };
            UpdateNodeCPCost(GetFirstRole("saz"), "sazh", costs);

            costs = new uint[] { 80, 90, 130, 275, 480, 1080, 915, 6000, 12000, 45000 };
            UpdateNodeCPCost(GetFirstRole("hop"), "hope", costs);

            costs = new uint[] { 70, 75, 320, 130, 400, 1040, 775, 6000, 12000, 45000 };
            UpdateNodeCPCost(GetFirstRole("van"), "vanille", costs);

            costs = new uint[] { 40, 200, 200, 200, 400, 1050, 865, 6000, 12000, 45000 };
            UpdateNodeCPCost(GetFirstRole("sno"), "snow", costs);

            costs = new uint[] { 70, 80, 350, 220, 450, 1200, 775, 6000, 12000, 45000 };
            UpdateNodeCPCost(GetFirstRole("fan"), "fang", costs);
        }

        private void UpdateNodeCPCost(string first, string chara, uint[] costs)
        {
            string[] roles = new string[] { "sen", "com", "rav", "syn", "sab", "med" };
            crystariums[chara].Values.Where(c => c.iCPCost == 0 || !primaryRoles[chara].Contains(c.iRole)).ForEach(c =>
            {
                if (c.iStage == 1)
                {
                    if ((int)c.iRole - 1 == roles.ToList().IndexOf(first))
                    {
                        if (crystariums[chara].Values.IndexOf(c) > crystariums[chara].Values.IndexOf(firstAbis[chara][c.iRole]) && c.iCPCost > 0)
                            c.iCPCost = costs[0];
                        else
                            c.iCPCost = 0;
                    }
                    else
                        c.iCPCost = costs[0];
                }
                else
                    c.iCPCost = costs[c.iStage - 1];
            });
        }

        private void RandomizeStatValues()
        {
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>("Treasures");
            string[] shortChars = new string[] { "lig", "fan", "hop", "saz", "sno", "van" };
            foreach (string c in shortChars)
            {
                StatPoints statPoints;
                Tuple<int, int>[] bounds = new Tuple<int, int>[] {
                        new Tuple<int, int>(20, 300),
                        new Tuple<int, int>(20, 300),
                        new Tuple<int, int>(20, 300)
                    };
                float[] weights = new float[] { 1, 1, 1 };
                int[] zeros = new int[] { 0, 0, 0 };
                int[] negs = new int[] { 0, 0, 0 };
                statPoints = new StatPoints(bounds, weights, zeros, negs, 0.5f);
                statPoints.Randomize(new int[] { 100, 100, 100 });
                charMults.Add(chars[shortChars.ToList().IndexOf(c)], Enumerable.Range(0, 3).Select(i => statPoints[i] / 100f).ToArray());
            }
        }

        private void RandomizeCrystariumStats(List<int[]> averageStats)
        {
            Dictionary<Role, float[]> roleMults = new Dictionary<Role, float[]>();
            for (int r = 1; r <= 6; r++)
            {
                Role role = (Role)r;
                StatPoints statPoints;
                Tuple<int, int>[] bounds = new Tuple<int, int>[] {
                        new Tuple<int, int>(20, 300),
                        new Tuple<int, int>(20, 300),
                        new Tuple<int, int>(20, 300)
                    };
                float[] weights = new float[] { 1, 1, 1 };
                int[] zeros = new int[] { 0, 0, 0 };
                int[] negs = new int[] { 0, 0, 0 };
                statPoints = new StatPoints(bounds, weights, zeros, negs, 0.5f);
                statPoints.Randomize(new int[] { 100, 100, 100 });
                roleMults.Add(role, Enumerable.Range(0, 3).Select(i => statPoints[i] / 100f).ToArray());
            }


            foreach (string chara in chars)
            {
                List<CrystariumType> types = new List<CrystariumType>() { CrystariumType.HP, CrystariumType.Strength, CrystariumType.Magic };
                crystariums[chara].Values.Where(c => types.Contains(c.iType)).ForEach(c =>
                {
                    c.iType = RandomNum.SelectRandomWeighted(types, t => (int)(Math.Sqrt(roleMults[c.iRole][types.IndexOf(t)]) * 100 * (t == CrystariumType.HP ? 1.4 : 1)));
                    c.iValue = (ushort)(averageStats[c.iStage - 1][types.IndexOf(c.iType)] * charMults[chara][types.IndexOf(c.iType)] * roleMults[c.iRole][types.IndexOf(c.iType)]);
                    if (crystariums[chara].Values.Where(other => other.iStage == c.iStage && other.iRole == c.iRole).Count() == 1)
                        c.iValue *= 10;
                    c.iValue = (ushort)Math.Max(1, RandomNum.RandInt((int)(c.iValue * 0.8), (int)(c.iValue * 1.2)));
                });
            }
        }

        private void RandomizeInitialStats()
        {
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>("Treasures");
            string[] shortChars = new string[] { "lig", "fan", "hop", "saz", "sno", "van" };
            int avgHP = (int)treasureRando.treasuresOrig.Values.Where(t => t.ID.StartsWith("z_ini_") && t.ID.EndsWith("_hp")).Select(t => (int)t.iItemCount).Average();
            int avgSTR = (int)treasureRando.treasuresOrig.Values.Where(t => t.ID.StartsWith("z_ini_") && t.ID.EndsWith("_str")).Select(t => (int)t.iItemCount).Average();
            int avgMAG = (int)treasureRando.treasuresOrig.Values.Where(t => t.ID.StartsWith("z_ini_") && t.ID.EndsWith("_mag")).Select(t => (int)t.iItemCount).Average();
            foreach (string c in shortChars)
            {
                string fullName = chars[shortChars.ToList().IndexOf(c)];
                treasureRando.treasures[$"z_ini_{c}_hp"].iItemCount = (uint)(avgHP * charMults[fullName][0]);
                treasureRando.treasures[$"z_ini_{c}_str"].iItemCount = (uint)(avgSTR * charMults[fullName][1]);
                treasureRando.treasures[$"z_ini_{c}_mag"].iItemCount = (uint)(avgMAG * charMults[fullName][2]);
            }
        }

        private void ShuffleNodesBetweenRoles()
        {
            foreach (string chara in chars)
            {
                List<CrystariumType> types = new List<CrystariumType>() { CrystariumType.Accessory, CrystariumType.ATBLevel, CrystariumType.HP, CrystariumType.Strength, CrystariumType.Magic };

                List<DataStoreCrystarium> nodes = crystariums[chara].Values.Where(c => types.Contains(c.iType) && c.iCPCost > 0).ToList().Shuffle().ToList();
                nodes.Shuffle((c1, c2) => c1.SwapStatsAbilities(c2));
            }
        }

        private void ShuffleNodes()
        {
            foreach (string chara in chars)
            {
                for (int r = 1; r <= 6; r++)
                {
                    Role role = (Role)r;

                    List<DataStoreCrystarium> nodes = crystariums[chara].Values.Where(c => c.iRole == role && c != firstAbis[chara][role] && c.iCPCost > 0).ToList().Shuffle().ToList();

                    nodes.Shuffle((c1, c2) =>
                    {
                        if (c1.iType == CrystariumType.RoleLevel && crystariums[chara].Values.IndexOf(c2) < crystariums[chara].Values.IndexOf(firstAbis[chara][c2.iRole]) || c2.iType == CrystariumType.RoleLevel && crystariums[chara].Values.IndexOf(c1) < crystariums[chara].Values.IndexOf(firstAbis[chara][c1.iRole]))
                            return;
                        else
                            c1.SwapStatsAbilities(c2);
                    });
                }
            }
        }

        private string GetFirstRole(string c)
        {
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>("Treasures");
            return treasureRando.treasures.Values.First(t => t.ID.StartsWith("z_ran_" + c) && treasureRando.itemLocations[t.ID].Traits.Contains("Same")).sItemResourceId_string.Substring($"rol_{c}_".Length);
        }
        public override HTMLPage GetDocumentation()
        {
            HTMLPage page = new HTMLPage("Crystarium", "template/documentation.html");

            chars.ForEach(name =>
            {
                page.HTMLElements.Add(new Table(name[0].ToString().ToUpper() + name.Substring(1),
                    new string[] { "Stage", "Commando", "Ravager", "Sentinel", "Synergist", "Saboteur", "Medic" }.ToList(),
                    new int[] { 10, 15, 15, 15, 15, 15, 15 }.ToList(),
                    Enumerable.Range(1, 10).Select(stage =>
                    {
                        List<string> list = new List<string>();
                        list.Add(stage.ToString());
                        list.AddRange(new string[] { "", "", "", "", "", "" });
                        foreach (Role role in Enum.GetValues(typeof(Role)))
                        {
                            List<string> additions = new List<string>();
                            if (role == Role.None)
                                continue;
                            List<DataStoreCrystarium> roleCrysts = crystariums[name].Values.Where(c => !c.ID.StartsWith("!") && c.iRole == role && c.iStage == stage).ToList();
                            int hp = roleCrysts.Where(c => c.iType == CrystariumType.HP).Sum(c => c.iValue);
                            int strength = roleCrysts.Where(c => c.iType == CrystariumType.Strength).Sum(c => c.iValue);
                            int magic = roleCrysts.Where(c => c.iType == CrystariumType.Magic).Sum(c => c.iValue);
                            int roleLevels = roleCrysts.Where(c => c.iType == CrystariumType.RoleLevel).Count();
                            int accessories = roleCrysts.Where(c => c.iType == CrystariumType.Accessory).Count();
                            int atbLevel = roleCrysts.Where(c => c.iType == CrystariumType.ATBLevel).Count();
                            List<string> abilities = roleCrysts.Where(c => c.iType == CrystariumType.Ability).Select(c => abilityData[c.sAbility_string].Name).ToList();
                            if (hp > 0)
                                additions.Add($"HP + {hp}");
                            if (strength > 0)
                                additions.Add($"Strength + {strength}");
                            if (magic > 0)
                                additions.Add($"Magic + {magic}");
                            if (roleLevels > 0)
                                additions.Add($"Role Level + {roleLevels}");
                            if (accessories > 0)
                                additions.Add($"Accessories + {accessories}");
                            if (atbLevel > 0)
                                additions.Add($"ATB Level");
                            additions.AddRange(abilities);
                            int roleCol = 0;
                            switch (role)
                            {
                                case Role.Commando:
                                    roleCol = 1;
                                    break;
                                case Role.Ravager:
                                    roleCol = 2;
                                    break;
                                case Role.Sentinel:
                                    roleCol = 3;
                                    break;
                                case Role.Synergist:
                                    roleCol = 4;
                                    break;
                                case Role.Medic:
                                    roleCol = 6;
                                    break;
                                case Role.Saboteur:
                                    roleCol = 5;
                                    break;
                            }
                            list[roleCol] = String.Join("<br>", additions);
                        }
                        return list;
                    })
                    .ToList(), name, false));
            });

            return page;
        }

        public override void Save()
        {
            Randomizers.SetProgressFunc("Saving Crystarium Data...", -1, 100);
            chars.ForEach(c => crystariums[c].SaveWDB(@"\db\crystal\crystal_" + c + ".wdb"));

        }
        public class AbilityData
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public Role Role { get; set; }
            public List<string> Characters { get; set; }
            public List<string> Traits { get; set; }
            public ItemReq Requirements { get; set; }
            public List<string> Incompatible { get; set; }

            private string[] chars = new string[] { "lightning", "fang", "hope", "sazh", "snow", "vanille" };
            public AbilityData(string[] row)
            {
                ID = row[0];
                Name = row[1];
                Role = row[2] == "" ? Role.None : Enum.GetValues(typeof(Role)).Cast<Role>().First(r => r.ToString().Substring(0, 3).ToUpper() == row[2]);
                Characters = row[3] == "" ? chars.ToList() : row[3].ToCharArray().Select(c => ToCharName(c)).ToList();
                Traits = row[4].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
                Requirements = ItemReq.Parse(row[5]);
                Incompatible = row[6].Split("|").Where(s => !string.IsNullOrEmpty(s)).ToList();
            }
            public string ToCharName(char id)
            {
                switch (id)
                {
                    case 'l':
                        return "lightning";
                    case 's':
                        return "snow";
                    case 'z':
                        return "sazh";
                    case 'h':
                        return "hope";
                    case 'f':
                        return "fang";
                    case 'v':
                        return "vanille";
                }
                return "";
            }
        }
    }
}
