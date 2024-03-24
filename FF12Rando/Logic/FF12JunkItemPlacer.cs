using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF12Rando;
public class FF12JunkItemPlacer : JunkItemPlacer<ItemLocation>
{
    private FF12ItemPlacer ParentPlacer { get; set; }
    private bool TomajWritPlaced { get; set; } = false;
    public FF12JunkItemPlacer(SeedGenerator generator, FF12ItemPlacer parentPlacer) : base(generator)
    {
        ParentPlacer = parentPlacer;
    }

    public override void PlaceItems()
    {
        TomajWritPlaced = false;
        base.PlaceItems();
    }

    public override (string Item, int Amount) GetNewItem((string Item, int Amount) orig)
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        string repItem = null;
        int amount = orig.Amount;

        if (!equipRando.itemData.ContainsKey(orig.Item) || equipRando.itemData[orig.Item].Rank > 10)
        {
            repItem = orig.Item;
        }
        else if (!TomajWritPlaced && FF12Flags.Items.WritGoals.SelectedValues.Contains(FF12Flags.Items.WritGoalAny))
        {
            repItem = "8070";
            TomajWritPlaced = true;
            amount = 1;
        }
        else
        {
            do
            {
                string category = equipRando.itemData[orig.Item1].Category;
                if (FF12Flags.Items.ReplaceAny.Enabled)
                {
                    category = equipRando.itemData.Values.Select(i => i.Category).Distinct()
                        .Where(c => c != "Key" && c != "Esper" && c != "Board" && c != "Ability").Shuffle().First();
                }

                int rankRange = FF12Flags.Items.ReplaceRank.Value;
                IEnumerable<ItemData> possible = equipRando.itemData.Values.Where(i =>
                    i.Category == category &&
                    i.Rank >= equipRando.itemData[orig.Item1].Rank - rankRange &&
                    i.Rank <= equipRando.itemData[orig.Item1].Rank + rankRange &&
                    i.Rank <= 10 &&
                    !i.Traits.Contains("Ignore"));

                repItem = RandomNum.SelectRandomOrDefault(possible)?.ID;
            } while (repItem == null);
        }

        return ModifyAmount((repItem, amount));
    }

    protected override HashSet<ItemLocation> GetEmptyMultiLocations()
    {
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        HashSet<DataStoreReward> emptyRewards = treasureRando.rewards.DataList.Where(r=>treasureRando.ItemLocations.ContainsKey($"{r.ID}:0")).ToHashSet();

        // Don't include any rewards that have an item set in one of its indices
        foreach(var loc in ParentPlacer.FinalPlacement.Keys)
        {
            if (loc is RewardLocation r)
            {
                emptyRewards.Remove(treasureRando.rewards[r.IntID - 0x9000]);
            }
        }

        return emptyRewards.Select(r => treasureRando.ItemLocations[$"{r.ID}:{RandomNum.RandInt(0, 2)}"]).ToHashSet();
    }
}
