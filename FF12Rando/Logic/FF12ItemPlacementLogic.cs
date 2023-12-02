using Bartz24.Data;
using Bartz24.RandoWPF;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FF12Rando;

public class FF12ItemPlacementLogic : ItemPlacementLogic<ItemLocation>
{
    private readonly TreasureRando treasureRando;

    public FF12ItemPlacementLogic(ItemPlacementAlgorithm<ItemLocation> algorithm, SeedGenerator randomizers) : base(algorithm)
    {
        treasureRando = randomizers.Get<TreasureRando>();
    }

    public override List<string> GetKeysToPlace()
    {
        List<string> possible = base.GetKeysToPlace().Where(l => ItemLocations[l] is not TreasureRando.TreasureData || treasureRando.treasuresToPlace.Contains(l)).ToList();

        if (FF12Flags.Items.WritGoals.SelectedValues.Contains(FF12Flags.Items.WritGoalCid2))
        {
            possible.RemoveAll(l => ItemLocations[l].Traits.Contains("WritCid2"));
        }

        return possible;
    }

    public override List<string> GetKeysAllowed()
    {
        List<string> possible = base.GetKeysAllowed().Where(l => ItemLocations[l] is not TreasureRando.TreasureData || treasureRando.treasuresAllowed.Contains(l)).ToList();

        if (FF12Flags.Items.WritGoals.SelectedValues.Contains(FF12Flags.Items.WritGoalCid2))
        {
            possible.RemoveAll(l => ItemLocations[l].Traits.Contains("WritCid2"));
        }

        return possible;
    }

    public override string AddHint(string location, string replacement, int itemDepth)
    {
        ItemLocations[location].Areas.Where(l => Algorithm.HintsByLocationsCount.ContainsKey(l)).ForEach(l => Algorithm.HintsByLocationsCount[l]--);

        if (IsHintable(replacement))
        {
            return treasureRando.AddHint(location).ToString();
        }

        return null;
    }

    public override int GetNextDepth(Dictionary<string, int> items, string location)
    {
        return ItemLocations[location].BaseDifficulty;
    }

    public override bool IsHintable(string location)
    {
        if (ItemLocations[location].Traits.Contains("Fake"))
        {
            return false;
        }

        if (FF12Flags.Items.KeyItems.SelectedKeys.Contains(GetLocationItem(location).Value.Item1) && treasureRando.IsImportantKeyItem(location))
        {
            return true;
        }

        if (treasureRando.IsWoT(location))
        {
            return true;
        }

        if (IsRandomizedChop(location))
        {
            return true;
        }

        if (IsRandomizedBlackOrb(location))
        {
            return true;
        }

        return false;
    }

    public bool IsRandomizedChop(string location)
    {
        if (!treasureRando.IsChopKeyItem(location))
        {
            return false;
        }

        string chopStr = ItemLocations[location].Traits.First(s => s.StartsWith("Chop"));
        int chop = int.Parse(chopStr.Substring(4));

        return chop <= FF12Flags.Items.KeyChops.Value;
    }

    public bool IsRandomizedBlackOrb(string location)
    {
        if (!treasureRando.IsBlackOrbKeyItem(location))
        {
            return false;
        }    

        string blackOrbStr = ItemLocations[location].Traits.First(s => s.StartsWith("BlackOrb"));
        int blackOrb = int.Parse(blackOrbStr.Substring(8));

        return blackOrb <= FF12Flags.Items.KeyBlackOrbs.Value;
    }

    public bool IsRandomizedTrophy(string location)
    {
        string item = GetLocationItem(location)?.Item1;
        if (item != null && Convert.ToInt32(item, 16) is >= 0x80B9 and <= 0x80D6)
        {
            return FF12Flags.Items.KeyItems.DictValues.Keys.Contains(item);
        }

        return false;
    }

    public override bool IsValid(string location, string replacement, Dictionary<string, int> items, List<string> areasAvailable)
    {
        return (!treasureRando.IsImportantKeyItem(replacement) || HasEnoughChars(location, items)) &&
            ItemLocations[location].IsValid(items) &&
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

    public override (string, int)? GetLocationItem(string key, bool orig = true)
    {
        switch (ItemLocations[key])
        {
            case TreasureRando.TreasureData t:
                return t.GetData(orig ? treasureRando.ebpAreasOrig[t.MapID].TreasureList[t.Index] : treasureRando.ebpAreas[t.MapID].TreasureList[t.Index]);
            case TreasureRando.RewardData t:
                if (t.Traits.Contains("Fake"))
                {
                    return null;
                }

                return t.GetData(orig ? treasureRando.rewardsOrig[t.IntID - 0x9000] : treasureRando.rewards[t.IntID - 0x9000]);
            case TreasureRando.StartingInvData s:
                PartyRando partyRando = treasureRando.Generator.Get<PartyRando>();
                return s.GetData(orig ? partyRando.partyOrig[s.IntID] : partyRando.party[s.IntID]);
            default:
                return base.GetLocationItem(key, orig);
        }
    }

    public override void SetLocationItem(string key, string item, int count)
    {
        LogSetItem(key, item, count);
        switch (ItemLocations[key])
        {
            case TreasureRando.TreasureData t:
                t.SetData(treasureRando.ebpAreas[t.MapID].TreasureList[t.Index], item, count);
                break;
            case TreasureRando.RewardData t:
                if (t.Traits.Contains("Fake"))
                {
                    break;
                }

                t.SetData(treasureRando.rewards[t.IntID - 0x9000], item, count);
                break;
            case TreasureRando.StartingInvData s:
                PartyRando partyRando = treasureRando.Generator.Get<PartyRando>();
                s.SetData(partyRando.party[s.IntID], item, count);
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

    protected override bool IsAllowedReplacement(string old, string rep)
    {
        if (ItemLocations[rep].Traits.Contains("Fake") || ItemLocations[old].Traits.Contains("Fake"))
        {
            return old == rep;
        }

        // If the old location is null, but the other in the reward can accept the replacement, then allow it.
        if (GetLocationItem(old) == null && treasureRando.IsImportantKeyItem(rep) && ItemLocations[old] is TreasureRando.RewardData rewardOld && rewardOld.Index > 0)
        {
            ItemLocation other = ItemLocations.Values.FirstOrDefault(l => l is TreasureRando.RewardData r && r.IntID == rewardOld.IntID && r.Index == (rewardOld.Index == 1 ? 2 : 1));
            if (GetLocationItem(other.ID) != null)
            {
                return IsAllowed(other.ID, rep);
            }
        }

        foreach (string item in FF12Flags.Items.KeyItems.DictValues.Keys)
        {
            if (!FF12Flags.Items.KeyItems.SelectedKeys.Contains(item) && (GetLocationItem(rep)?.Item1 == item || GetLocationItem(old)?.Item1 == item))
            {
                return old == rep;
            }
        }

        if ((treasureRando.IsChopKeyItem(old) && !IsRandomizedChop(old)) || (treasureRando.IsChopKeyItem(t: rep) && !IsRandomizedChop(rep)))
        {
            return old == rep;
        }

        if((treasureRando.IsBlackOrbKeyItem(old) && !IsRandomizedBlackOrb(old)) || (treasureRando.IsBlackOrbKeyItem(t: rep) && !IsRandomizedBlackOrb(rep)))
        {
            return old == rep;
        }

        if (ItemLocations[old].Traits.Contains("Missable"))
        {
            if (treasureRando.IsImportantKeyItem(rep) || treasureRando.IsAbility(rep) || treasureRando.IsWoT(rep))
            {
                return false;
            }
        }

        List<string> specialTraits = new();

        if (ItemLocations[old].Traits.Contains("Hunt"))
        {
            specialTraits.Add("Hunt");
            if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF12Flags.Items.KeyPlaceHunt.Enabled)
            {
                return false;
            }
        }

        if (ItemLocations[old].Traits.Contains("ClanRank"))
        {
            specialTraits.Add("ClanRank");
            if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF12Flags.Items.KeyPlaceClanRank.Enabled)
            {
                return false;
            }
        }

        if (ItemLocations[old].Traits.Contains("ClanBoss"))
        {
            specialTraits.Add("ClanBoss");
            if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF12Flags.Items.KeyPlaceClanBoss.Enabled)
            {
                return false;
            }
        }

        if (ItemLocations[old].Traits.Contains("ClanEsper"))
        {
            specialTraits.Add("ClanEsper");
            if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF12Flags.Items.KeyPlaceClanEsper.Enabled)
            {
                return false;
            }
        }

        if (ItemLocations[old].Traits.Contains("Grindy"))
        {
            specialTraits.Add("Grindy");
            if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF12Flags.Items.KeyPlaceGrindy.Enabled)
            {
                return false;
            }
        }

        if (ItemLocations[old].Traits.Contains("Hidden"))
        {
            specialTraits.Add("Hidden");
            if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF12Flags.Items.KeyPlaceHidden.Enabled)
            {
                return false;
            }
        }

        if (ItemLocations[old] is TreasureRando.RewardData reward)
        {
            if (reward.Index == 0 && GetLocationItem(rep).Value.Item1 != "Gil")
            {
                return false;
            }

            if (reward.Index > 0 && GetLocationItem(rep).Value.Item1 == "Gil")
            {
                return false;
            }
        }

        if (ItemLocations[old] is TreasureRando.TreasureData)
        {
            if (GetLocationItem(rep)?.Item1 != "Gil" && GetLocationItem(rep)?.Item2 > 1)
            {
                return false;
            }
        }

        if (ItemLocations[old] is TreasureRando.StartingInvData)
        {
            if (GetLocationItem(rep)?.Item1 == "Gil")
            {
                return false;
            }
        }

        return (!treasureRando.IsImportantKeyItem(rep) || treasureRando.IsImportantKeyItem(old) || FF12Flags.Items.KeyPlaceTreasure.Enabled || specialTraits.Count != 0)
&& (treasureRando.IsImportantKeyItem(rep) || !treasureRando.IsImportantKeyItem(old) || FF12Flags.Items.KeyPlaceTreasure.Enabled || specialTraits.Count != 0);
    }
    public override void RemoveLikeItemsFromRemaining(string replacement, List<string> remaining)
    {
        if (IsRandomizedBlackOrb(replacement))
        {
            List<string> orbs = remaining.Where(l => IsRandomizedBlackOrb(l)).ToList();
            remaining.RemoveAll(rem => orbs.Contains(rem) || orbs.Select(child => ((TreasureRando.RewardData)ItemLocations[child]).Parent.ID).Contains(rem));
        }
        else if (IsRandomizedTrophy(replacement))
        {
            List<string> trophies = remaining.Where(l => IsRandomizedTrophy(l)).ToList();
            remaining.RemoveAll(rem => trophies.Contains(rem) || trophies.Select(child => ((TreasureRando.RewardData)ItemLocations[child]).Parent.ID).Contains(rem));
        }
        else if (IsRandomizedChop(replacement))
        {
            List<string> chops = remaining.Where(l => IsRandomizedChop(l)).ToList();
            remaining.RemoveAll(rem => chops.Contains(rem) || chops.Select(child => ((TreasureRando.RewardData)ItemLocations[child]).Parent.ID).Contains(rem));
        }
        else
        {
            base.RemoveLikeItemsFromRemaining(replacement, remaining);
        }
    }

    public override int GetPlacementDifficultyIndex()
    {
        return FF12Flags.Items.KeyDepth.SelectedIndex;
    }

    public override void Clear()
    {
        treasureRando.hints.ForEach(l => l.Clear());
    }

    public override Dictionary<string, int> GetItemsAvailable(Dictionary<string, string> placement)
    {
        Dictionary<string, int> dict = base.GetItemsAvailable(placement);
        placement.Keys.Where(l => ItemLocations[l] is TreasureRando.RewardData).Select(l => (TreasureRando.RewardData)ItemLocations[l]).ForEach(l =>
        {
            if (l.FakeItems.Count > 0 && l.Parent == l)
            {
                l.FakeItems.ForEach(item =>
                {
                    if (dict.ContainsKey(item))
                    {
                        dict[item] += 1;
                    }
                    else
                    {
                        dict.Add(item, 1);
                    }
                });
            }
        });

        PartyRando partyRando = treasureRando.Generator.Get<PartyRando>();
        int[] chars = partyRando.CharacterMapping.Select(s => dict.ContainsKey(s) ? dict[s] : 0).ToArray();
        partyRando.CharacterMapping.Where(s => dict.ContainsKey(s)).ForEach(s => dict.Remove(s));
        Enumerable.Range(0, 6).Where(i => chars[i] > 0).ForEach(i => dict.Add(partyRando.CharacterMapping[partyRando.Characters[i]], chars[i]));

        return dict;
    }
    private int GetCharCount(Dictionary<string, int> items)
    {
        int count = 0;
        if (items.ContainsKey("Vaan") && items["Vaan"] > 0)
        {
            count++;
        }

        if (items.ContainsKey("Ashe") && items["Ashe"] > 0)
        {
            count++;
        }

        if (items.ContainsKey("Fran") && items["Fran"] > 0)
        {
            count++;
        }

        if (items.ContainsKey("Balthier") && items["Balthier"] > 0)
        {
            count++;
        }

        if (items.ContainsKey("Basch") && items["Basch"] > 0)
        {
            count++;
        }

        if (items.ContainsKey("Penelo") && items["Penelo"] > 0)
        {
            count++;
        }

        if (items.ContainsKey("Guest") && items["Guest"] > 0)
        {
            count++;
        }

        return count;
    }

    private bool HasEnoughChars(string location, Dictionary<string, int> items)
    {
        if (FF12Flags.Items.CharacterScale.Enabled)
        {
            int charCount = GetCharCount(items);
            int diff = ItemLocations[location].BaseDifficulty;

            if (diff >= 7)
            {
                return charCount >= 6;
            }

            if (diff >= 5)
            {
                return charCount >= 5;
            }

            if (diff >= 4)
            {
                return charCount >= 4;
            }

            if (diff >= 3)
            {
                return charCount >= 3;
            }
        }

        return true;
    }
}
