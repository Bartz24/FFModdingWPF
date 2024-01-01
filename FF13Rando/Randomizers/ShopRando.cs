using Bartz24.Data;
using Bartz24.Docs;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using System.Collections.Generic;
using System.Linq;
using static FF13Rando.EquipRando;

namespace FF13Rando;

public class ShopRando : Randomizer
{
    public DataStoreWDB<DataStoreShop> shops = new();
    public ShopRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading Shop Data...");
        shops.LoadWDB(Generator, "13", @"\db\resident\shop.wdb");
    }
    public override void Randomize()
    {
        //TODO: add option to lock in high quality upgrading components?
        RandoUI.SetUIProgressIndeterminate("Randomizing Shop Data...");

        if (FF13Flags.Items.ShopContents.FlagEnabled)
        {
            FF13Flags.Items.ShopContents.SetRand();
            List<string> shopIDs = new()
            {
                "shop_acc_a",
                "shop_acc_b",
                "shop_acc_c",
                "shop_acc_d",
                "shop_item_a",
                "shop_item_b",
                "shop_mat_a",
                "shop_mat_b",
                "shop_mat_c",
                "shop_mat_d",
                "shop_wea_a",
                "shop_wea_b",
                "shop_wea_d"
            };

            int vanillaCount = shopIDs.Select(id => shops.Values.Where(s => s.ID.StartsWith(id)).Last()).Select(s => s.GetItems().Count).Sum();

            Dictionary<string, List<string>> newShopContents = shopIDs.ToDictionary(s => s, _ => new List<string>());
            StatValues shopWeights = new(shopIDs.Count);
            shopWeights.Randomize(99);

            EquipRando equipRando = Generator.Get<EquipRando>();
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
                {
                    newShopContents[id] = RandomNum.ShuffleLocalized(newShopContents[id].OrderBy(item => item is "it_potion" or "it_phenxtal" ? 1 : equipRando.itemData[item].Rank).ToList(), 2);
                }

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
        HTMLPage page = new("Shops", "template/documentation.html");
        Dictionary<string, string> shopNames = new()
        {
            { "shop_acc_a", "B&W Outfitters" },
            { "shop_acc_b", "Magical Moments" },
            { "shop_acc_c", "Moogleworks" },
            { "shop_acc_d", "Sanctum Labs" },
            { "shop_item_a", "Unicorn Mart" },
            { "shop_item_b", "Eden Pharmaceuticals" },
            { "shop_mat_a", "Creature Comforts" },
            { "shop_mat_b", "The Motherlode" },
            { "shop_mat_c", "Lenora's Garage" },
            { "shop_mat_d", "R&D Depot" },
            { "shop_wea_a", "Up In Arms" },
            { "shop_wea_b", "Plautus's Workshop" },
            { "shop_wea_d", "Gilgamesh, Inc." }
        };

        shopNames.Keys.ForEach(id =>
        {
            List<DataStoreShop> storesForID = shops.Values.Where(s => s.ID.StartsWith(id)).ToList();
            page.HTMLElements.Add(new Table(shopNames[id], (new string[] { "Item", "Shop Level Required" }).ToList(), (new int[] { 50, 50 }).ToList(), storesForID.Last().GetItems().Select(itemID =>
            {
                string name = GetItemName(itemID);
                int unlock = storesForID.IndexOf(storesForID.Where(s => s.GetItems().Contains(itemID)).First()) + 1;
                return (name, unlock);
            }).OrderBy(t => t.unlock).Select(t => new string[] { t.name, t.unlock.ToString() }.ToList()
            ).ToList(), "shops"));
        });

        pages.Add("shops", page);
        return pages;
    }

    private string GetItemName(string itemID)
    {
        EquipRando equipRando = Generator.Get<EquipRando>();
        TextRando textRando = Generator.Get<TextRando>();
        string name;
        if (itemID == "")
        {
            name = "Gil";
        }
        else
        {
            name = textRando.mainSysUS[equipRando.items[itemID].sItemNameStringId_string];
            if (name.Contains("{End}"))
            {
                name = name.Substring(0, name.IndexOf("{End}"));
            }
        }

        return name;
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving Shop Data...");
        shops.SaveWDB(Generator, @"\db\resident\shop.wdb");
    }
}
