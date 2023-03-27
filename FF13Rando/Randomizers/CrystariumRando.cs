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
        public Dictionary<string, Dictionary<Role, string>> firstNodes = new Dictionary<string, Dictionary<Role, string>>();

        public CrystariumRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            Randomizers.SetUIProgress("Loading Crystarium Data...", 0, 100);
            primaryRoles.Add("lightning", new Role[] { Role.Commando, Role.Ravager, Role.Medic });
            primaryRoles.Add("fang", new Role[] { Role.Commando, Role.Sentinel, Role.Saboteur });
            primaryRoles.Add("snow", new Role[] { Role.Commando, Role.Ravager, Role.Sentinel });
            primaryRoles.Add("sazh", new Role[] { Role.Commando, Role.Ravager, Role.Synergist });
            primaryRoles.Add("hope", new Role[] { Role.Ravager, Role.Synergist, Role.Medic });
            primaryRoles.Add("vanille", new Role[] { Role.Ravager, Role.Saboteur, Role.Medic });


            crystariums = chars.ToDictionary(c => c, c => new DataStoreWDB<DataStoreCrystarium>());
            chars.ForEach(c => crystariums[c].LoadWDB("13", @"\db\crystal\crystal_" + c + ".wdb"));

            Randomizers.SetUIProgress("Loading Crystarium Data...", 80, 100);
            FileHelpers.ReadCSVFile(@"data\abilities.csv", row =>
            {
                AbilityData a = new AbilityData(row);
                abilityData.Add(a.ID, a);
            }, FileHelpers.CSVFileHeader.HasHeader);

            firstNodes = chars.ToDictionary(c => c, c => new Role[] { Role.Commando, Role.Ravager, Role.Sentinel, Role.Saboteur, Role.Synergist, Role.Medic }.ToDictionary(r => r, _ => ""));
            firstNodes["lightning"][Role.Commando] = "cr_ltat01910000";
            firstNodes["lightning"][Role.Ravager] = "cr_ltbl01010000";
            firstNodes["lightning"][Role.Sentinel] = "cr_ltdf01010000";
            firstNodes["lightning"][Role.Saboteur] = "cr_ltja01010000";
            firstNodes["lightning"][Role.Synergist] = "cr_lteh01010000";
            firstNodes["lightning"][Role.Medic] = "cr_lthl03010000";

            firstNodes["fang"][Role.Commando] = "cr_faat01910000";
            firstNodes["fang"][Role.Ravager] = "cr_fabl01010000";
            firstNodes["fang"][Role.Sentinel] = "cr_fadf01010000";
            firstNodes["fang"][Role.Saboteur] = "cr_faja01010000";
            firstNodes["fang"][Role.Synergist] = "cr_faeh01010000";
            firstNodes["fang"][Role.Medic] = "cr_fahl01010000";

            firstNodes["snow"][Role.Commando] = "cr_snat01010000";
            firstNodes["snow"][Role.Ravager] = "cr_snbl01010000";
            firstNodes["snow"][Role.Sentinel] = "cr_sndf01910000";
            firstNodes["snow"][Role.Saboteur] = "cr_snja01010000";
            firstNodes["snow"][Role.Synergist] = "cr_sneh01010000";
            firstNodes["snow"][Role.Medic] = "cr_snhl01010000";

            firstNodes["sazh"][Role.Commando] = "cr_szat03010000";
            firstNodes["sazh"][Role.Ravager] = "cr_szbl01910000";
            firstNodes["sazh"][Role.Sentinel] = "cr_szdf01010000";
            firstNodes["sazh"][Role.Saboteur] = "cr_szja01010000";
            firstNodes["sazh"][Role.Synergist] = "cr_szeh02010000";
            firstNodes["sazh"][Role.Medic] = "cr_szhl01010000";

            firstNodes["hope"][Role.Commando] = "cr_hpat01010000";
            firstNodes["hope"][Role.Ravager] = "cr_hpbl01010000";
            firstNodes["hope"][Role.Sentinel] = "cr_hpdf01010000";
            firstNodes["hope"][Role.Saboteur] = "cr_hpja01010000";
            firstNodes["hope"][Role.Synergist] = "cr_hpeh01910000";
            firstNodes["hope"][Role.Medic] = "cr_hphl01010000";

            firstNodes["vanille"][Role.Commando] = "cr_vaat01010000";
            firstNodes["vanille"][Role.Ravager] = "cr_vabl01010000";
            firstNodes["vanille"][Role.Sentinel] = "cr_vadf01010000";
            firstNodes["vanille"][Role.Saboteur] = "cr_vaja02010000";
            firstNodes["vanille"][Role.Synergist] = "cr_vaeh01010000";
            firstNodes["vanille"][Role.Medic] = "cr_vahl01910000";
        }
        public override void Randomize()
        {
            Randomizers.SetUIProgress("Randomizing Crystarium Data...", 0, 100);
            List<int[]> averageStats = GetAverageStats();
            MoveFirstAbilities();
            UpdateCPCosts();

            Randomizers.SetUIProgress("Randomizing Crystarium Data...", 10, 100);
            if (FF13Flags.Stats.RandCrystAbi.FlagEnabled)
            {
                FF13Flags.Stats.RandCrystAbi.SetRand();
                RandomizeAbilities();
                RandomNum.ClearRand();
            }

            Randomizers.SetUIProgress("Randomizing Crystarium Data...", 20, 100);
            if (FF13Flags.Stats.RandCrystStat.FlagEnabled)
            {
                FF13Flags.Stats.RandCrystStat.SetRand();
                RandomizeStatValues();
                RandomNum.ClearRand();
            }

            Randomizers.SetUIProgress("Randomizing Crystarium Data...", 40, 100);
            if (FF13Flags.Stats.ShuffleCrystMisc.Enabled)
            {
                FF13Flags.Stats.RandInitStats.SetRand();
                ShuffleNodesBetweenRoles();
                RandomNum.ClearRand();
            }

            Randomizers.SetUIProgress("Randomizing Crystarium Data...", 50, 100);
            if (FF13Flags.Stats.ShuffleCrystRole.FlagEnabled)
            {
                FF13Flags.Stats.ShuffleCrystRole.SetRand();
                ShuffleNodes();
                RandomNum.ClearRand();
            }

            Randomizers.SetUIProgress("Randomizing Crystarium Data...", 80, 100);
            if (FF13Flags.Stats.RandCrystStat.FlagEnabled)
            {
                FF13Flags.Stats.RandCrystStat.SetRand();
                RandomizeCrystariumStats(averageStats);
                RandomNum.ClearRand();
            }

            Randomizers.SetUIProgress("Randomizing Crystarium Data...", 90, 100);
            if (FF13Flags.Stats.RandInitStats.FlagEnabled)
            {
                FF13Flags.Stats.RandInitStats.SetRand();
                RandomizeInitialStats();
                RandomNum.ClearRand();
            }

            Randomizers.SetUIProgress("Randomizing Crystarium Data...", 95, 100);
            ApplyCPCostModifiers();
            if (FF13Flags.Stats.ScaledCPCosts.FlagEnabled)
            {
                ScaledCPCosts();
            }
            RoundCPCosts();
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

        private void MoveFirstAbilities()
        {
            foreach (string chara in chars)
            {

                for (int r = 1; r <= 6; r++)
                {
                    DataStoreCrystarium firstNode = crystariums[chara][firstNodes[chara][(Role)r]];
                    List<DataStoreCrystarium> stageAbis = crystariums[chara].Values.Where(c => c.iType == CrystariumType.Ability && c.iStage == firstNode.iStage).ToList();
                    DataStoreCrystarium abiNode = stageAbis.First(c => c.iRole == (Role)r && abilityData[c.sAbility_string].Role != Role.None);
                    firstNode.SwapStatsAbilities(abiNode);
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
                        DataStoreCrystarium first = crystariums[chara][firstNodes[chara][(Role)r]];
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
                        List<DataStoreCrystarium> nodes = remaining.Where(c => c.iStage == stage).Shuffle();

                        foreach (DataStoreCrystarium node in nodes)
                        {
                            IEnumerable<AbilityData> required = abilityData.Values.Where(a => a.Characters.Contains(chara) && a.Traits.Contains("Required") && !abilitiesPlaced.Contains(a.ID));
                            if (remaining.Count == required.Count())
                            {
                                node.sAbility_string = required.Shuffle().Select(a => a.ID).First();
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
                .Shuffle().Select(a => a.ID).FirstOrDefault();
        }
        private string GetNextAbilityRole(List<string> used, string chara, Role role, bool allowTech, bool allowAuto)
        {
            return abilityData.Values.Where(a => !used.Contains(a.ID) && (allowAuto || !a.Traits.Contains("Auto")) && (allowTech && a.Role == Role.None || a.Role == role) && a.Requirements.IsValid(used.ToDictionary(s => s, _ => 1)) && a.Incompatible.Intersect(used).Count() == 0 && a.Characters.Contains(chara))
                .Shuffle().Select(a => a.ID).FirstOrDefault();
        }

        private void UpdateCPCosts()
        {
            uint[] costs = new uint[] { 25, 50, 100, 250, 500, 750, 1000, 4000, 10000, 30000 };
            UpdateNodeCPCost(GetFirstRole("lig"), "lightning", costs);

            UpdateNodeCPCost(GetFirstRole("saz"), "sazh", costs);

            UpdateNodeCPCost(GetFirstRole("hop"), "hope", costs);

            UpdateNodeCPCost(GetFirstRole("van"), "vanille", costs);

            UpdateNodeCPCost(GetFirstRole("sno"), "snow", costs);

            UpdateNodeCPCost(GetFirstRole("fan"), "fang", costs);
        }

        private void UpdateNodeCPCost(string first, string chara, uint[] costs)
        {
            string[] roles = { "sen", "com", "rav", "syn", "sab", "med" };
            crystariums[chara].Values.ForEach(c =>
            {
                if (c.iStage == 1)
                {
                    if ((int)c.iRole - 1 == roles.ToList().IndexOf(first))
                    {
                        if (c.ID != firstNodes[chara][c.iRole])
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
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>();
            string[] shortChars = { "lig", "fan", "hop", "saz", "sno", "van" };
            foreach (string c in shortChars)
            {
                StatPoints statPoints;
                (int, int)[] bounds = {
                    (20, 300),
                    (20, 300),
                    (20, 300)
                };
                float[] weights = { 1, 1, 1 };
                int[] chances = { 1, 1, 1 };
                int[] zeros = { 0, 0, 0 };
                int[] negs = { 0, 0, 0 };
                statPoints = new StatPoints(bounds, weights, chances, zeros, negs, 0.5f);
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
                (int, int)[] bounds = {
                    (20, 300),
                    (20, 300),
                    (20, 300)
                };
                float[] weights = { 1, 1, 1 };
                int[] chances = { 1, 1, 1 };
                int[] zeros = { 0, 0, 0 };
                int[] negs = { 0, 0, 0 };
                statPoints = new StatPoints(bounds, weights, chances, zeros, negs, 0.5f);
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
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>();
            string[] shortChars = { "lig", "fan", "hop", "saz", "sno", "van" };
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

                if (FF13Flags.Stats.RandCrystAbiAll.Enabled)
                    types.Add(CrystariumType.Ability);

                List<DataStoreCrystarium> nodes = crystariums[chara].Values.Where(c => types.Contains(c.iType) && c.iCPCost > 0 && c.ID != firstNodes[chara][c.iRole]).Shuffle();
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

                    List<DataStoreCrystarium> nodes = crystariums[chara].Values.Where(c => c.iRole == role && c.ID != firstNodes[chara][role] && c.iCPCost > 0).Shuffle();

                    nodes.Shuffle((c1, c2) =>
                    {
                        c1.SwapStatsAbilities(c2);
                    });
                }
            }
        }

        private string GetFirstRole(string c)
        {
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>();
            return treasureRando.treasures.Values.First(t => t.ID.StartsWith("z_ran_" + c) && treasureRando.itemLocations[t.ID].Traits.Contains("Same")).sItemResourceId_string.Substring($"rol_{c}_".Length);
        }
        public override Dictionary<string, HTMLPage> GetDocumentation()
        {
            Dictionary<string, HTMLPage> pages = base.GetDocumentation();
            HTMLPage page = new HTMLPage("Crystarium", "template/documentation.html");

            chars.ForEach(name =>
            {
                Dictionary<Role, (int, int, int)> roleTotals = crystariums[name].Values.Where(c => !c.ID.StartsWith("!")).GroupBy(c => c.iRole).Where(g => g.Key != Role.None).OrderBy(g => g.Key).Select(roleGroup =>
                {
                    var hpTotal = roleGroup.Where(c => c.iType == CrystariumType.HP).Sum(c => c.iValue);
                    var strengthTotal = roleGroup.Where(c => c.iType == CrystariumType.Strength).Sum(c => c.iValue);
                    var magicTotal = roleGroup.Where(c => c.iType == CrystariumType.Magic).Sum(c => c.iValue);
                    return (roleGroup.Key, (hpTotal, strengthTotal, magicTotal));
                }).ToDictionary(kv => kv.Item1, kv => kv.Item2);
                int charHpTotal = roleTotals.Values.Select(t => t.Item1).Sum();
                int charStrTotal = roleTotals.Values.Select(t => t.Item2).Sum();
                int charMagTotal = roleTotals.Values.Select(t => t.Item3).Sum();
                Func<(int, int, int), string> writeStatsForDisplay = ((int, int, int) input) =>
                    string.Join("<br/>", $"HP + {input.Item1}", $"Strength + {input.Item2}", $"Magic + {input.Item3}");
                page.HTMLElements.Add(new Table(name[0].ToString().ToUpper() + name.Substring(1),
                    new string[] { "Stage", "Commando", "Ravager", "Sentinel", "Synergist", "Saboteur", "Medic", "Totals" }.ToList(),
                    new int[] { 4, 14, 14, 14, 14, 14, 14, 12 }.ToList(),
                    Enumerable.Range(1, 10).Select(stage =>
                    {
                        List<string> list = new List<string>();
                        list.Add(stage.ToString());
                        list.AddRange(new string[] { "", "", "", "", "", "" });
                        int hpTotal = 0;
                        int strengthTotal = 0;
                        int magicTotal = 0;
                        List<string> totalAdditions = new List<string>();
                        foreach (Role role in Enum.GetValues(typeof(Role)))
                        {
                            List<string> additions = new List<string>();
                            if (role == Role.None)
                                continue;
                            List<DataStoreCrystarium> roleCrysts = crystariums[name].Values.Where(c => !c.ID.StartsWith("!") && c.iRole == role && c.iStage == stage).ToList();
                            int hp = roleCrysts.Where(c => c.iType == CrystariumType.HP).Sum(c => c.iValue);
                            hpTotal += hp;
                            int strength = roleCrysts.Where(c => c.iType == CrystariumType.Strength).Sum(c => c.iValue);
                            strengthTotal += strength;
                            int magic = roleCrysts.Where(c => c.iType == CrystariumType.Magic).Sum(c => c.iValue);
                            magicTotal += magic;
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
                            list[roleCol] = string.Join("<br/>", additions);
                        }
                        //Stage total stats
                        list.Add(writeStatsForDisplay((hpTotal, strengthTotal, magicTotal)));
                        return list;
                    })
                    .Append(new List<string>() { "Totals",
                        writeStatsForDisplay(roleTotals[Role.Commando]), writeStatsForDisplay(roleTotals[Role.Ravager]), writeStatsForDisplay(roleTotals[Role.Sentinel]),
                        writeStatsForDisplay(roleTotals[Role.Synergist]), writeStatsForDisplay(roleTotals[Role.Saboteur]), writeStatsForDisplay(roleTotals[Role.Medic]),
                        "Overall Totals<br/>" + writeStatsForDisplay((charHpTotal, charStrTotal, charMagTotal))
                    })
                    .ToList(), name, false));
            });

            pages.Add("crystarium", page);
            return pages;
        }

        private void ApplyCPCostModifiers()
        {
            foreach (string chara in chars)
            {
                crystariums[chara].Values.Where(c => c.IsSideNode || c.iType == CrystariumType.Ability || c.iType == CrystariumType.Accessory || c.iType == CrystariumType.ATBLevel || c.iType == CrystariumType.RoleLevel).ForEach(c =>
                {
                    c.iCPCost = (uint)(c.iCPCost * (c.iStage == 10 ? 2f : (1.2f + c.iStage * 0.07f)));
                });

                Enumerable.Range(1, 10).ForEach(stage =>
                {
                    Enumerable.Range((int)Role.Sentinel, 6).ForEach(role =>
                    {
                        IEnumerable<DataStoreCrystarium> enumerable = crystariums[chara].Values.Where(c => c.iStage == stage && c.iRole == (Role)role);
                        if (enumerable.Count() == 1)
                            enumerable.First().iCPCost *= 4;
                    });
                });
            }
        }

        private void ScaledCPCosts()
        {
            foreach (string chara in chars)
            {
                crystariums[chara].Values.ForEach(c =>
                {
                    if (c.iCPCost > 0)
                    {
                        int cpCost = (int)Math.Floor(c.iCPCost * Math.Max(0.5, Math.Min(1, 1.08684 * Math.Exp(-0.08664 * c.iStage))));
                    }
                });
            }
        }

        private void RoundCPCosts()
        {
            foreach (string chara in chars)
            {
                crystariums[chara].Values.ForEach(c =>
                {
                    if (c.iCPCost > 0)
                    {
                        int interval;
                        if (c.iCPCost < 100)
                            interval = 5;
                        else if (c.iCPCost < 1000)
                            interval = 10;
                        else if (c.iCPCost < 5000)
                            interval = 250;
                        else if (c.iCPCost < 10000)
                            interval = 500;
                        else if (c.iCPCost < 50000)
                            interval = 1000;
                        else
                            interval = 5000;
                        c.iCPCost = Math.Max(1, (uint)MathHelpers.RoundToInterval((int)c.iCPCost, interval));
                    }
                });
            }
        }

        public override void Save()
        {
            Randomizers.SetUIProgress("Saving Crystarium Data...", -1, 100);
            chars.ForEach(c => crystariums[c].SaveWDB(@"\db\crystal\crystal_" + c + ".wdb"));

        }
        public class AbilityData : CSVDataRow
        {
            [RowIndex(0)]
            public string ID { get; set; }
            [RowIndex(0)]
            public string Name { get; set; }
            public Role Role { get; set; }
            public List<string> Characters { get; set; }
            [RowIndex(4)]
            public List<string> Traits { get; set; }
            [RowIndex(5)]
            public ItemReq Requirements { get; set; }
            [RowIndex(6)]
            public List<string> Incompatible { get; set; }

            private string[] chars = { "lightning", "fang", "hope", "sazh", "snow", "vanille" };
            public AbilityData(string[] row) : base(row)
            {
                Role = row[2] == "" ? Role.None : Enum.GetValues(typeof(Role)).Cast<Role>().First(r => r.ToString().Substring(0, 3).ToUpper() == row[2]);
                Characters = row[3] == "" ? chars.ToList() : row[3].ToCharArray().Select(c => ToCharName(c)).ToList();
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
