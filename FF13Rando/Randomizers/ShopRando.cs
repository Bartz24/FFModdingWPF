using Bartz24.Data;
using Bartz24.FF13Series;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using Bartz24.Docs;
using static FF13Rando.EquipRando;

namespace FF13Rando
{
    public class ShopRando : Randomizer
    {
        public DataStoreWDB<DataStoreShop> shops = new DataStoreWDB<DataStoreShop>();
        public ShopRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            Randomizers.SetProgressFunc("Loading Shop Data...", -1, 100);
            shops.LoadWDB("13", @"\db\resident\shop.wdb");
        }
        public override void Randomize(Action<int> progressSetter)
        {
            //TODO: add option to lock in high quality upgrading components?
            Randomizers.SetProgressFunc("Randomizing Shop Data...", -1, 100);

            if (FF13Flags.Items.ShopContents.FlagEnabled)
            {
                FF13Flags.Items.ShopContents.SetRand();
                List<string> shopIDs = new List<string>();
                shopIDs.Add("shop_acc_a");
                shopIDs.Add("shop_acc_b");
                shopIDs.Add("shop_acc_c");
                shopIDs.Add("shop_acc_d");
                shopIDs.Add("shop_item_a");
                shopIDs.Add("shop_item_b");
                shopIDs.Add("shop_mat_a");
                shopIDs.Add("shop_mat_b");
                shopIDs.Add("shop_mat_c");
                shopIDs.Add("shop_mat_d");
                shopIDs.Add("shop_wea_a");
                shopIDs.Add("shop_wea_b");
                shopIDs.Add("shop_wea_d");

                int vanillaCount = shopIDs.Select(id => shops.Values.Where(s => s.ID.StartsWith(id)).Last()).Select(s => s.GetItems().Count).Sum();

                Dictionary<string, List<string>> newShopContents = shopIDs.ToDictionary(s => s, _ => new List<string>());
                StatValues shopWeights = new StatValues(shopIDs.Count);
                shopWeights.Randomize(99);

                EquipRando equipRando = Randomizers.Get<EquipRando>();
                equipRando.itemData.Values.Where(i => i.Traits.Contains("Force")).ForEach(i =>
                {
                    string shop = FF13Flags.Items.AnyShop.Enabled ? RandomNum.SelectRandomWeighted(shopIDs, s => newShopContents[s].Count < 32 ? shopWeights[shopIDs.IndexOf(s)] : 0) : i.DefaultShop;
                    newShopContents[shop].Add(i.ID);
                });

                List<ItemData> remaining = equipRando.itemData.Values.Where(i => !i.Traits.Contains("Force")).Shuffle().ToList();

                shopIDs.ForEach(id =>
                {
                    int tiers = shops.Values.Where(s => s.ID.StartsWith(id)).Count();

                    if (tiers > newShopContents[id].Count)
                    {
                        remaining.Where(i => FF13Flags.Items.AnyShop.Enabled || i.DefaultShop == id).Take(tiers - newShopContents[id].Count).ForEach(i =>
                        {
                            newShopContents[id].Add(i.ID);
                            remaining.Remove(i);
                        });
                    }
                });

                remaining.Take((int)(vanillaCount * 1.5) - newShopContents.Values.Select(l => l.Count).Sum()).ForEach(i =>
                {
                    string shop = FF13Flags.Items.AnyShop.Enabled ? RandomNum.SelectRandomWeighted(shopIDs, s => newShopContents[s].Count < 32 ? shopWeights[shopIDs.IndexOf(s)] : 0) : i.DefaultShop;
                    newShopContents[shop].Add(i.ID);
                });

                shopIDs.ForEach(id =>
                {
                    int tiers = shops.Values.Where(s => s.ID.StartsWith(id)).Count();
                    List<int> sizes = Enumerable.Range(1, newShopContents[id].Count).Shuffle().Take(tiers).OrderBy(i => i).ToList();
                    sizes[sizes.Count - 1] = newShopContents[id].Count;

                    newShopContents[id] = newShopContents[id].Shuffle().ToList();
                    if (FF13Flags.Items.ShopContentOrder.Enabled)
                        newShopContents[id] = RandomNum.ShuffleLocalized(newShopContents[id].OrderBy(item => item == "it_potion" || item == "it_phenxtal" ? 1 : equipRando.itemData[item].Rank).ToList(), 2);

                    for (int i = 0; i < tiers; i++)
                    {
                        shops[$"{id}{i}"].SetItems(newShopContents[id].Take(sizes[i]).OrderBy(item => equipRando.itemData[item].SortIndex).ToList());
                    }
                });

                RandomNum.ClearRand();
            }
        }
        public override Dictionary<string, HTMLPage> GetDocumentation()
        {
            Dictionary<string, HTMLPage> pages = base.GetDocumentation();
            HTMLPage page = new HTMLPage("Shops", "template/documentation.html");
            Dictionary<string, string> shopNames = new Dictionary<string, string>();
            shopNames.Add("shop_acc_a", "B&W Outfitters");
            shopNames.Add("shop_acc_b", "Magical Moments");
            shopNames.Add("shop_acc_c", "Moogleworks");
            shopNames.Add("shop_acc_d", "Sanctum Labs");
            shopNames.Add("shop_item_a", "Unicorn Mart");
            shopNames.Add("shop_item_b", "Eden Pharmaceuticals");
            shopNames.Add("shop_mat_a", "Creature Comforts");
            shopNames.Add("shop_mat_b", "The Motherlode");
            shopNames.Add("shop_mat_c", "Lenora's Garage");
            shopNames.Add("shop_mat_d", "R&D Depot");
            shopNames.Add("shop_wea_a", "Up In Arms");
            shopNames.Add("shop_wea_b", "Plautus's Workshop");
            shopNames.Add("shop_wea_d", "Gilgamesh, Inc.");

            shopNames.Keys.ForEach(id =>
            {
                List<DataStoreShop> storesForID = shops.Values.Where(s => s.ID.StartsWith(id)).ToList();
                page.HTMLElements.Add(new Table(shopNames[id], (new string[] { "Item", "Shop Level Required" }).ToList(), (new int[] { 50, 50 }).ToList(), storesForID.Last().GetItems().Select(itemID =>
                {
                    string name = GetItemName(itemID);
                    int unlock = storesForID.IndexOf(storesForID.Where(s => s.GetItems().Contains(itemID)).First()) + 1;
                    return (name, unlock);
                }).OrderBy(t => t.Item2).Select(t => new string[] { t.Item1, t.Item2.ToString() }.ToList()
                ).ToList(), "shops"));
            });

            pages.Add("shops", page);
            return pages;
        }

        private string GetItemName(string itemID)
        {
            EquipRando equipRando = Randomizers.Get<EquipRando>();
            TextRando textRando = Randomizers.Get<TextRando>();
            string name;
            if (itemID == "")
                name = "Gil";
            else
            {
                name = textRando.mainSysUS[equipRando.items[itemID].sItemNameStringId_string];
                if (name.Contains("{End}"))
                    name = name.Substring(0, name.IndexOf("{End}"));
            }

            return name;
        }

        public override void Save()
        {
            Randomizers.SetProgressFunc("Saving Shop Data...", -1, 100);
            shops.SaveWDB(@"\db\resident\shop.wdb");
        }
    }
}
