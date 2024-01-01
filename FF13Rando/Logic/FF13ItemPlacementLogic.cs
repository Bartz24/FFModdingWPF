using Bartz24.RandoWPF;
using System.Collections.Generic;
using System.Linq;

namespace FF13Rando;

public class FF13ItemPlacementLogic : ItemPlacementLogic<FF13ItemLocation>
{
    private readonly TreasureRando treasureRando;

    public FF13ItemPlacementLogic(ItemPlacementAlgorithm<FF13ItemLocation> algorithm, SeedGenerator randomizers) : base(algorithm)
    {
        treasureRando = randomizers.Get<TreasureRando>();
    }

    public override string AddHint(string location, string replacement, int itemDepth)
    {
        ItemLocations[location].Areas.ForEach(l => Algorithm.HintsByLocationsCount[l]--);
        return null;
    }

    public override int GetNextDepth(Dictionary<string, int> items, string location)
    {
        return ItemLocations[location].BaseDifficulty;
    }

    public override bool IsHintable(string location)
    {
        return false;
    }

    public override bool IsValid(string location, string replacement, Dictionary<string, int> items, List<string> areasAvailable)
    {
        return ItemLocations[location].IsValid(items) &&
            FF13RandoHelpers.AreCrystariumReqsMet(ItemLocations[location], items) &&
            ItemLocations[location].Areas.Intersect(areasAvailable).Count() > 0 &&
            IsAllowed(location, replacement);
    }

    public override void RemoveHint(string hint, string location)
    {
        ItemLocations[location].Areas.ForEach(l => Algorithm.HintsByLocationsCount[l]++);
    }
    public override void RemoveLikeItemsFromRemaining(string replacement, List<string> remaining)
    {
        if (treasureRando.IsShop(replacement) && FF13Flags.Items.KeyShops.Enabled)
        {
            remaining.RemoveAll(rem => treasureRando.IsShop(rem));
        }
        else if ((treasureRando.IsInitRole(replacement) || treasureRando.IsOtherRole(replacement)) && FF13Flags.Items.KeyInitRoles.Enabled && FF13Flags.Items.KeyRoles.Enabled)
        {
            remaining.RemoveAll(rem => treasureRando.IsInitRole(rem) || treasureRando.IsOtherRole(rem));
        }
        else if (treasureRando.IsInitRole(replacement) && FF13Flags.Items.KeyInitRoles.Enabled)
        {
            remaining.RemoveAll(rem => treasureRando.IsInitRole(rem));
        }
        else if (treasureRando.IsOtherRole(replacement) && FF13Flags.Items.KeyRoles.Enabled)
        {
            remaining.RemoveAll(rem => treasureRando.IsOtherRole(rem));
        }
        else if (treasureRando.IsStage(replacement) && FF13Flags.Items.KeyStages.Enabled)
        {
            remaining.RemoveAll(rem => treasureRando.IsStage(rem));
        }
        else if (treasureRando.IsEidolon(replacement) && FF13Flags.Items.KeyEidolith.Enabled)
        {
            remaining.RemoveAll(rem => treasureRando.IsEidolon(rem));
        }
        else
        {
            base.RemoveLikeItemsFromRemaining(replacement, remaining);
        }
    }

    public override bool RequiresDepthLogic(string location)
    {
        return treasureRando.IsImportantKeyItem(location);
    }

    public override bool RequiresLogic(string location)
    {
        return ItemLocations[location].Traits.Contains("Same")
|| ItemLocations[location].GetItem(true).Value.Item1 == "" || treasureRando.IsImportantKeyItem(location);
    }

    public override List<string> GetNewAreasAvailable(Dictionary<string, int> items, List<string> soFar)
    {
        return ItemLocations.Values.SelectMany(t => t.Areas).Distinct().ToList();
    }

    protected override bool IsAllowedReplacement(string old, string rep)
    {
        if (!FF13Flags.Items.KeyEidolith.Enabled && (treasureRando.IsEidolon(rep) || treasureRando.IsEidolon(old)))
        {
            return old == rep;
        }

        if (!FF13Flags.Items.KeyInitRoles.Enabled && (treasureRando.IsInitRole(rep) || treasureRando.IsInitRole(old)))
        {
            return old == rep;
        }

        if (!FF13Flags.Items.KeyRoles.Enabled && (treasureRando.IsOtherRole(rep) || treasureRando.IsOtherRole(old)))
        {
            return old == rep;
        }

        if (!FF13Flags.Items.KeyReins.Enabled && (treasureRando.IsGysahlReins(rep) || treasureRando.IsGysahlReins(old)))
        {
            return old == rep;
        }

        if (!FF13Flags.Items.KeyShops.Enabled && (treasureRando.IsShop(rep) || treasureRando.IsShop(old)))
        {
            return old == rep;
        }

        if (!FF13Flags.Items.KeyStages.Enabled && (treasureRando.IsStage(rep) || treasureRando.IsStage(old)))
        {
            return old == rep;
        }

        if (ItemLocations[rep].Traits.Contains("Same") || ItemLocations[old].Traits.Contains("Same"))
        {
            return old == rep;
        }

        if (ItemLocations[old].Traits.Contains("Missable"))
        {
            if (treasureRando.IsImportantKeyItem(rep))
            {
                return false;
            }
        }

        if (ItemLocations[old].Traits.Contains("Repeatable"))
        {
            if (!treasureRando.IsRepeatableAllowed(rep))
            {
                return treasureRando.IsShop(rep) && !FF13Flags.Items.KeyPlaceTreasure.Enabled && !FF13Flags.Items.KeyPlaceMissions.Enabled;
            }
        }

        List<string> specialTraits = new();

        if (ItemLocations[old].Traits.Contains("Mission"))
        {
            specialTraits.Add("Mission");
            if (ItemLocations[rep].GetItem(true).Value.Item1 == "")
            {
                return false;
            }

            if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF13Flags.Items.KeyPlaceMissions.Enabled)
            {
                return false;
            }
        }

        if (ItemLocations[old] is BattleDropData)
        {
            if (ItemLocations[rep].GetItem(true).Value.Item1 == "")
            {
                return false;
            }
        }
        else if (ItemLocations[old] is EnemyDropData)
        {
            if (ItemLocations[rep].GetItem(true).Value.Item1 == "")
            {
                return false;
            }
        }

        return (!treasureRando.IsImportantKeyItem(rep) || treasureRando.IsImportantKeyItem(old) || FF13Flags.Items.KeyPlaceTreasure.Enabled || specialTraits.Count != 0)
&& (treasureRando.IsImportantKeyItem(rep) || !treasureRando.IsImportantKeyItem(old) || FF13Flags.Items.KeyPlaceTreasure.Enabled || specialTraits.Count != 0);
    }

    public override int GetPlacementDifficultyIndex()
    {
        return FF13Flags.Items.KeyDepth.SelectedIndex;
    }
    public override void Clear()
    {

    }
}
