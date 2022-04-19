using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF12Rando
{
    public class FF12ItemPlacementAlgorithm : ItemPlacementAlgorithm<FF12ItemLocation>
    {
        TreasureRando treasureRando;

        Dictionary<string, int> AreaDepths = new Dictionary<string, int>();

        public FF12ItemPlacementAlgorithm(Dictionary<string, FF12ItemLocation> itemLocations, List<string> hintsByLocations, RandomizerManager randomizers, int maxFail) : base(itemLocations, hintsByLocations, maxFail)
        {
            treasureRando = randomizers.Get<TreasureRando>("Treasures");
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
            ItemLocations[location].Areas.Where(l => HintsByLocationsCount.ContainsKey(l)).ForEach(l => HintsByLocationsCount[l]--);

            if (IsHintable(replacement))
            {
                int index = Enumerable.Range(0, treasureRando.hints.Count).First(i => treasureRando.hints[i].Count == treasureRando.hints.Select(l => l.Count).Min());
                treasureRando.hints[index].Add(location);
                return index.ToString();
            }
            return null;
        }
        private int GetReqsMaxDepth(ItemReq req)
        {
            return req.GetPossibleRequirements().Select(item =>
            {
                return Placement.Keys.Where(t => GetLocationItem(Placement[t]).Item1 == item).Select(t => Depths[t]).DefaultIfEmpty(0).Max();
            }).DefaultIfEmpty(0).Max();
        }

        public override int GetNextDepth(Dictionary<string, int> items, string location)
        {
            int val = ItemLocations[location].Difficulty;

            return RandomNum.RandInt(Math.Max(0, val - 2), val + 2);
        }

        public override bool IsHintable(string location)
        {
            if (treasureRando.IsImportantKeyItem(location))
                return true;
            if (FF12Flags.Other.HintAbilities.FlagEnabled && treasureRando.IsAbility(location))
                return true;
            return false;
        }

        public override bool IsValid(string location, string replacement, string area, Dictionary<string, int> items, List<string> areasAvailable)
        {
            return ItemLocations[location].IsValid(items) &&
                (area == null || ItemLocations[location].Areas.Contains(area)) &&
                IsAllowed(location, replacement);
        }

        public override void RemoveHint(string hint, string location)
        {
            ItemLocations[location].Areas.Where(l => HintsByLocationsCount.ContainsKey(l)).ForEach(l => HintsByLocationsCount[l]++);
            int index = int.Parse(hint);
            treasureRando.hints[index].Remove(location);
        }

        public override bool RequiresDepthLogic(string location)
        {
            return treasureRando.IsImportantKeyItem(location);
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

        public override bool IsAllowed(string old, string rep)
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


        public override Tuple<string, int> SelectNext(Dictionary<string, int> items, List<string> possible, string rep)
        {
            if (FF12Flags.Items.KeyDepth.SelectedValue == FF12Flags.Items.KeyDepth.Values[FF12Flags.Items.KeyDepth.Values.Count - 1])
            {
                IOrderedEnumerable<KeyValuePair<string, int>> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, s)).OrderByDescending(p => p.Value);
                KeyValuePair<string, int> pair = possDepths.First();
                return new Tuple<string, int>(pair.Key, pair.Value);
            }
            else
            {
                int index = FF12Flags.Items.KeyDepth.Values.IndexOf(FF12Flags.Items.KeyDepth.SelectedValue);
                float expBase = 1;
                if (index == 0)
                    expBase = 1;
                if (index == 1)
                    expBase = 1.1f;
                if (index == 2)
                    expBase = 1.2f;
                if (index == 3)
                    expBase = 1.5f;
                Dictionary<string, int> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, s));
                string next = RandomNum.SelectRandomWeighted(possible, s => (long)Math.Pow(expBase, possDepths[s]));
                return new Tuple<string, int>(next, possDepths[next]);
            }
        }
        public override void Clear()
        {
            treasureRando.hints.ForEach(l => l.Clear());
        }

        private Dictionary<int, string> fakeItemTracking;

        protected override Dictionary<string, int> GetItemsAvailable(Dictionary<string, string> placement)
        {
            Dictionary<string, int> dict = base.GetItemsAvailable(placement);
            fakeItemTracking = new Dictionary<int, string>();
            placement.Keys.Where(l => ItemLocations[l] is TreasureRando.RewardData).Select(l => (TreasureRando.RewardData)ItemLocations[l]).ForEach(l =>
            {
                if (!fakeItemTracking.ContainsKey(l.IntID))
                {
                    l.FakeItems.ForEach(item =>
                    {
                        if (dict.ContainsKey(item))
                            dict[item] += 1;
                        else
                            dict.Add(item, 1);
                    });
                    fakeItemTracking.Add(l.IntID, l.ID);
                }
            });

            int[] chars = treasureRando.characterMapping.Select(s => dict.ContainsKey(s) ? dict[s] : 0).ToArray();
            treasureRando.characterMapping.Where(s => dict.ContainsKey(s)).ForEach(s => dict.Remove(s));
            Enumerable.Range(0, 6).Where(i => chars[i] > 0).ForEach(i => dict.Add(treasureRando.characterMapping[treasureRando.characters[i]], chars[i]));


            return dict;
        }
    }
}
