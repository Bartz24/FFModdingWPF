using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13Rando
{
    public class FF13ItemPlacementAlgorithm : ItemPlacementAlgorithm<ItemLocation>
    {
        TreasureRando treasureRando;

        Dictionary<string, double> AreaMults = new Dictionary<string, double>();

        public FF13ItemPlacementAlgorithm(Dictionary<string, ItemLocation> itemLocations, List<string> hintsByLocations, RandomizerManager randomizers, int maxFail) : base(itemLocations, hintsByLocations, maxFail)
        {
            treasureRando = randomizers.Get<TreasureRando>("Treasures");
        }

        public override string AddHint(Dictionary<string, int> items, string location, string replacement, int itemDepth)
        {
            ItemLocations[location].Areas.ForEach(l => HintsByLocationsCount[l]--);
            return null;
        }

        public override int GetNextDepth(Dictionary<string, int> items, string location)
        {
            return ItemLocations[location].Difficulty;
        }

        public override bool IsHintable(string location)
        {
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
            ItemLocations[location].Areas.ForEach(l => HintsByLocationsCount[l]++);
        }

        public override bool RequiresDepthLogic(string location)
        {
            return treasureRando.IsImportantKeyItem(location);
        }

        public override bool RequiresLogic(string location)
        {
            if (ItemLocations[location].Traits.Contains("Same"))
                return true;
            if (GetLocationItem(location).Item1 == "")
                return true;
            if (treasureRando.IsImportantKeyItem(location))
                return true;
            return false;
        }

        public override Tuple<string, int> GetLocationItem(string key, bool orig = true)
        {
            switch (ItemLocations[key])
            {
                case TreasureRando.TreasureData t:
                    return t.GetData(orig ? treasureRando.treasuresOrig[key] : treasureRando.treasures[key]);
                case TreasureRando.BattleData b:
                    BattleRando battleRando = treasureRando.Randomizers.Get<BattleRando>("Battles");
                    return b.GetData(orig ? battleRando.btsceneOrig[key] : battleRando.btscene[key]);
                case TreasureRando.EnemyData e:
                    EnemyRando enemyRando = treasureRando.Randomizers.Get<EnemyRando>("Enemies");
                    return e.GetData(orig ? enemyRando.charaSpecOrig[key] : enemyRando.charaSpec[key]);
                default:
                    return base.GetLocationItem(key, orig);
            }
        }

        public override void SetLocationItem(string key, string item, int count)
        {
            switch (ItemLocations[key])
            {
                case TreasureRando.TreasureData t:
                    t.SetData(treasureRando.treasures[key], item, count);
                    break;
                case TreasureRando.BattleData b:
                    BattleRando battleRando = treasureRando.Randomizers.Get<BattleRando>("Battles");
                    b.SetData(battleRando.btscene[key], item, count);
                    break;
                case TreasureRando.EnemyData e:
                    EnemyRando enemyRando = treasureRando.Randomizers.Get<EnemyRando>("Enemies");
                    e.SetData(enemyRando.charaSpec[key], item, count);
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
            if (!FF13Flags.Items.KeyEidolith.Enabled && (treasureRando.IsEidolon(rep) || treasureRando.IsEidolon(old)))
                return old == rep;
            if (!FF13Flags.Items.KeyInitRoles.Enabled && (treasureRando.IsInitRole(rep) || treasureRando.IsInitRole(old)))
                return old == rep;
            if (!FF13Flags.Items.KeyRoles.Enabled && (treasureRando.IsOtherRole(rep) || treasureRando.IsOtherRole(old)))
                return old == rep;
            if (!FF13Flags.Items.KeyReins.Enabled && (treasureRando.IsGysahlReins(rep) || treasureRando.IsGysahlReins(old)))
                return old == rep;
            if (!FF13Flags.Items.KeyShops.Enabled && (treasureRando.IsShop(rep) || treasureRando.IsShop(old)))
                return old == rep;
            if (!FF13Flags.Items.KeyStages.Enabled && (treasureRando.IsStage(rep) || treasureRando.IsStage(old)))
                return old == rep;

            if (ItemLocations[rep].Traits.Contains("Same") || ItemLocations[old].Traits.Contains("Same"))
                return old == rep;

            if (ItemLocations[old].Traits.Contains("Missable"))
            {
                if (treasureRando.IsImportantKeyItem(rep))
                    return false;
            }
            if (ItemLocations[old].Traits.Contains("Repeatable"))
            {
                if (!treasureRando.IsRepeatableAllowed(rep))
                    return false;
            }
            if (ItemLocations[old].Traits.Contains("Mission"))
            {
                if (GetLocationItem(rep).Item1 == "")
                    return false;
                if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF13Flags.Items.KeyPlaceMissions.Enabled)
                    return false;
            }
            if (ItemLocations[old] is TreasureRando.BattleData)
            {
                if (GetLocationItem(rep).Item1 == "")
                    return false;
            }
            if (ItemLocations[old] is TreasureRando.EnemyData)
            {
                if (GetLocationItem(rep).Item1 == "")
                    return false;
            }

            if (treasureRando.IsImportantKeyItem(rep) && !treasureRando.IsImportantKeyItem(old) && !FF13Flags.Items.KeyPlaceTreasure.Enabled)
                return false;
            if (!treasureRando.IsImportantKeyItem(rep) && treasureRando.IsImportantKeyItem(old) && !FF13Flags.Items.KeyPlaceTreasure.Enabled)
                return false;

            return true;
        }

        public override Tuple<string, int> SelectNext(Dictionary<string, int> items, List<string> possible, string rep)
        {
            if (FF13Flags.Items.KeyDepth.SelectedValue == FF13Flags.Items.KeyDepth.Values[FF13Flags.Items.KeyDepth.Values.Count - 1])
            {
                IOrderedEnumerable<KeyValuePair<string, int>> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, s)).OrderByDescending(p => p.Value);
                KeyValuePair<string, int> pair = possDepths.First();
                return new Tuple<string, int>(pair.Key, pair.Value);
            }
            else
            {
                int index = FF13Flags.Items.KeyDepth.Values.IndexOf(FF13Flags.Items.KeyDepth.SelectedValue);
                float expBase = 1;
                if (index == 0)
                    expBase = 1;
                if (index == 1)
                    expBase = 1.05f;
                if (index == 2)
                    expBase = 1.1f;
                if (index == 3)
                    expBase = 1.25f;
                Dictionary<string, int> possDepths = possible.ToDictionary(s => s, s => GetNextDepth(items, s));
                string next = RandomNum.SelectRandomWeighted(possible, s => (long)(Math.Pow(expBase, possDepths[s]) * GetAreaMult(s) * 100d));
                return new Tuple<string, int>(next, possDepths[next]);
            }
        }

        public double GetAreaMult(string location)
        {
            return Math.Max(0.01, ItemLocations[location].Areas.Select(a => AreaMults[a]).Average());
        }
        public void SetAreaMults(Dictionary<string, double> mults)
        {
            AreaMults = mults;
        }

        public override void Clear()
        {

        }
    }
}
