using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF12Rando
{
    public class FF12ItemPlacementLogic : ItemPlacementLogic<ItemLocation>
    {
        TreasureRando treasureRando;

        public FF12ItemPlacementLogic(ItemPlacementAlgorithm<ItemLocation> algorithm, RandomizerManager randomizers) : base(algorithm)
        {
            treasureRando = randomizers.Get<TreasureRando>();
        }

        public override List<string> GetKeysToPlace()
        {
            return base.GetKeysToPlace().Where(l => !(ItemLocations[l] is TreasureRando.TreasureData) || treasureRando.treasuresToPlace.Contains(l)).ToList();
        }

        public override List<string> GetKeysAllowed()
        {
            return base.GetKeysAllowed().Where(l => !(ItemLocations[l] is TreasureRando.TreasureData) || treasureRando.treasuresAllowed.Contains(l)).ToList();
        }

        public override string AddHint(Dictionary<string, int> items, string location, string replacement, int itemDepth)
        {
            ItemLocations[location].Areas.Where(l => Algorithm.HintsByLocationsCount.ContainsKey(l)).ForEach(l => Algorithm.HintsByLocationsCount[l]--);

            if (IsHintable(replacement))
            {
                int index = Enumerable.Range(0, treasureRando.hints.Count).First(i => treasureRando.hints[i].Count == treasureRando.hints.Select(l => l.Count).Min());
                treasureRando.hints[index].Add(location);
                return index.ToString();
            }
            return null;
        }

        public override int GetNextDepth(Dictionary<string, int> items, string location)
        {
            return ItemLocations[location].Difficulty;
        }

        public override bool IsHintable(string location)
        {
            if (FF12Flags.Items.KeyMain.Enabled && treasureRando.IsMainKeyItem(location))
                return true;
            if (FF12Flags.Items.KeySide.Enabled && treasureRando.IsSideKeyItem(location))
                return true;
            if (FF12Flags.Items.KeyWrit.Enabled && treasureRando.IsWoTItem(location))
                return true;
            if (FF12Flags.Items.KeyGrindy.Enabled && treasureRando.IsGrindyKeyItem(location))
                return true;
            if (FF12Flags.Items.KeyOrb.Enabled && treasureRando.IsBlackOrbKeyItem(location))
                return true;
            if (FF12Flags.Items.KeyHunt.Enabled && treasureRando.IsHuntKeyItem(location))
                return true;
            if (FF12Flags.Items.KeyTrophy.Enabled && treasureRando.IsHuntClubKeyItem(location))
                return true;
            if (FF12Flags.Other.HintAbilities.FlagEnabled && treasureRando.IsAbility(location))
                return true;
            return false;
        }

        public override bool IsValid(string location, string replacement, Dictionary<string, int> items, List<string> areasAvailable)
        {
            if (treasureRando.IsImportantKeyItem(replacement) && !HasEnoughChars(replacement, items))
                return false;

            return ItemLocations[location].IsValid(items) &&
                ItemLocations[location].Areas.Intersect(areasAvailable).Count() > 0 &&
                IsAllowed(location, replacement);
        }

        public override void RemoveHint(string hint, string location)
        {
            ItemLocations[location].Areas.Where(l => Algorithm.HintsByLocationsCount.ContainsKey(l)).ForEach(l => Algorithm.HintsByLocationsCount[l]++);
            int index = int.Parse(hint);
            treasureRando.hints[index].Remove(location);
        }

        public override bool RequiresDepthLogic(string location)
        {
            return treasureRando.IsImportantKeyItem(location) || ItemLocations[location].Traits.Contains("Fake");
        }

        public override bool RequiresLogic(string location)
        {
            return true;
        }

        public override Tuple<string, int> GetLocationItem(string key, bool orig = true)
        {
            switch (ItemLocations[key])
            {
                case TreasureRando.TreasureData t:
                    return t.GetData(orig ? treasureRando.ebpAreasOrig[t.MapID].TreasureList[t.Index] : treasureRando.ebpAreas[t.MapID].TreasureList[t.Index]);
                case TreasureRando.RewardData t:
                    if (t.Traits.Contains("Fake"))
                        return null;
                    return t.GetData(orig ? treasureRando.rewardsOrig[t.IntID - 0x9000] : treasureRando.rewards[t.IntID - 0x9000]);
                default:
                    return base.GetLocationItem(key, orig);
            }
        }

        public override void SetLocationItem(string key, string item, int count)
        {
            switch (ItemLocations[key])
            {
                case TreasureRando.TreasureData t:
                    t.SetData(treasureRando.ebpAreas[t.MapID].TreasureList[t.Index], item, count);
                    break;
                case TreasureRando.RewardData t:
                    if (t.Traits.Contains("Fake"))
                        break;
                    t.SetData(treasureRando.rewards[t.IntID - 0x9000], item, count);
                    break;
                default:
                    base.SetLocationItem(key, item, count);
                    break;
            }
        }

        public override List<string> GetNewAreasAvailable(Dictionary<string, int> items, List<string> soFar)
        {
            return ItemLocations.Values.SelectMany(t => t.Areas).Distinct().ToList();
        }

        public override bool IsAllowed(string old, string rep, bool orig = true)
        {
            if (ItemLocations[rep].Traits.Contains("Fake") || ItemLocations[old].Traits.Contains("Fake"))
                return old == rep;
            if (!FF12Flags.Items.KeyMain.Enabled && (treasureRando.IsMainKeyItem(rep) || treasureRando.IsMainKeyItem(old)))
                return old == rep;
            if (!FF12Flags.Items.KeySide.Enabled && (treasureRando.IsSideKeyItem(rep) || treasureRando.IsSideKeyItem(old)))
                return old == rep;
            if (!FF12Flags.Items.KeyWrit.Enabled && (treasureRando.IsWoTItem(rep) || treasureRando.IsWoTItem(old)))
                return old == rep;
            if (!FF12Flags.Items.KeyGrindy.Enabled && (treasureRando.IsGrindyKeyItem(rep) || treasureRando.IsGrindyKeyItem(old)))
                return old == rep;
            if (!FF12Flags.Items.KeyOrb.Enabled && (treasureRando.IsBlackOrbKeyItem(rep) || treasureRando.IsBlackOrbKeyItem(old)))
                return old == rep;
            if (!FF12Flags.Items.KeyHunt.Enabled && (treasureRando.IsHuntKeyItem(rep) || treasureRando.IsHuntKeyItem(old)))
                return old == rep;
            if (!FF12Flags.Items.KeyTrophy.Enabled && (treasureRando.IsHuntClubKeyItem(rep) || treasureRando.IsHuntClubKeyItem(old)))
                return old == rep;

            if (ItemLocations[old].Traits.Contains("Missable"))
            {
                if (treasureRando.IsImportantKeyItem(rep))
                    return false;
            }
            if (ItemLocations[old].Traits.Contains("Hunt"))
            {
                if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF12Flags.Items.KeyPlaceHunt.Enabled)
                    return false;
            }
            if (ItemLocations[old].Traits.Contains("ClanRank"))
            {
                if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF12Flags.Items.KeyPlaceClanRank.Enabled)
                    return false;
            }
            if (ItemLocations[old].Traits.Contains("ClanBoss"))
            {
                if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF12Flags.Items.KeyPlaceClanBoss.Enabled)
                    return false;
            }
            if (ItemLocations[old].Traits.Contains("ClanEsper"))
            {
                if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF12Flags.Items.KeyPlaceClanEsper.Enabled)
                    return false;
            }
            if (ItemLocations[old].Traits.Contains("Grindy"))
            {
                if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF12Flags.Items.KeyPlaceGrindy.Enabled)
                    return false;
            }
            if (ItemLocations[old] is TreasureRando.RewardData)
            {
                TreasureRando.RewardData reward = (TreasureRando.RewardData)ItemLocations[old];
                if (reward.Index == 0 && GetLocationItem(rep).Item1 != "Gil")
                    return false;
                if (reward.Index > 0 && GetLocationItem(rep).Item1 == "Gil")
                    return false;
            }
            if (ItemLocations[old] is TreasureRando.TreasureData)
            {
                if (GetLocationItem(rep).Item1 != "Gil" && GetLocationItem(rep).Item2 > 1)
                    return false;
            }
            if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF12Flags.Items.KeyPlaceTreasure.Enabled)
                return false;
            if (!treasureRando.IsImportantKeyItem(rep) && treasureRando.IsImportantKeyItem(old) && !FF12Flags.Items.KeyPlaceTreasure.Enabled)
                return false;
            return true;
        }
        public override void RemoveLikeItemsFromRemaining(string replacement, List<string> remaining)
        {
            if (treasureRando.IsBlackOrbKeyItem(replacement) && FF12Flags.Items.KeyOrb.Enabled)
                remaining.RemoveAll(rem => treasureRando.IsBlackOrbKeyItem(rem));
            else if (treasureRando.IsHuntClubKeyItem(replacement) && GetLocationItem(replacement)?.Item1 != "80B8" && FF12Flags.Items.KeyTrophy.Enabled)
                remaining.RemoveAll(rem => treasureRando.IsHuntClubKeyItem(rem) && GetLocationItem(replacement)?.Item1 != "80B8");
            else if (treasureRando.IsGrindyKeyItem(replacement) && GetLocationItem(replacement)?.Item1 == "2113" && FF12Flags.Items.KeyGrindy.Enabled)
                remaining.RemoveAll(rem => treasureRando.IsGrindyKeyItem(replacement) && GetLocationItem(replacement)?.Item1 == "2113");
            else
                base.RemoveLikeItemsFromRemaining(replacement, remaining);
        }

        public override int GetPlacementDifficultyIndex()
        {
            return FF12Flags.Items.KeyDepth.SelectedIndex;
        }

        public override void Clear()
        {
            treasureRando.hints.ForEach(l => l.Clear());
        }

        public Dictionary<int, string> FakeItemTracking { get; set; }

        public override Dictionary<string, int> GetItemsAvailable(Dictionary<string, string> placement)
        {
            Dictionary<string, int> dict = base.GetItemsAvailable(placement);
            FakeItemTracking = new Dictionary<int, string>();
            placement.Keys.Where(l => ItemLocations[l] is TreasureRando.RewardData).Select(l => (TreasureRando.RewardData)ItemLocations[l]).ForEach(l =>
            {
                if (!FakeItemTracking.ContainsKey(l.IntID))
                {
                    l.FakeItems.ForEach(item =>
                    {
                        if (dict.ContainsKey(item))
                            dict[item] += 1;
                        else
                            dict.Add(item, 1);
                    });
                    FakeItemTracking.Add(l.IntID, l.ID);
                }
            });

            int[] chars = treasureRando.characterMapping.Select(s => dict.ContainsKey(s) ? dict[s] : 0).ToArray();
            treasureRando.characterMapping.Where(s => dict.ContainsKey(s)).ForEach(s => dict.Remove(s));
            Enumerable.Range(0, 6).Where(i => chars[i] > 0).ForEach(i => dict.Add(treasureRando.characterMapping[treasureRando.characters[i]], chars[i]));


            return dict;
        }
        private int GetCharCount(Dictionary<string, int> items)
        {
            int count = 0;
            if (items.ContainsKey("Vaan") && items["Vaan"] > 0)
                count++;
            if (items.ContainsKey("Ashe") && items["Ashe"] > 0)
                count++;
            if (items.ContainsKey("Fran") && items["Fran"] > 0)
                count++;
            if (items.ContainsKey("Balthier") && items["Balthier"] > 0)
                count++;
            if (items.ContainsKey("Basch") && items["Basch"] > 0)
                count++;
            if (items.ContainsKey("Penelo") && items["Penelo"] > 0)
                count++;
            if (items.ContainsKey("Guest") && items["Guest"] > 0)
                count++;
            return count;
        }

        private bool HasEnoughChars(string location, Dictionary<string, int> items)
        {
            int charCount = GetCharCount(items);
            if (FF12Flags.Items.CharacterScale.Enabled)
            {
                if (ItemLocations[location].Difficulty >= 7)
                    return charCount >= 6;
                if (ItemLocations[location].Difficulty >= 6)
                    return charCount >= 5;
                if (ItemLocations[location].Difficulty >= 5)
                    return charCount >= 4;
                if (ItemLocations[location].Difficulty >= 3)
                    return charCount >= 3;
            }
            return true;
        }
    }
}
