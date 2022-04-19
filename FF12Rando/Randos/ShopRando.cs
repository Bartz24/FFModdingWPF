using Bartz24.Docs;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bartz24.Data;
using Bartz24.FF12;

namespace FF12Rando
{
    public class ShopRando : Randomizer
    {
        public DataStoreBPShop shops;
        public DataStoreBPSection<DataStoreBazaar> bazaars;

        public ShopRando(RandomizerManager randomizers) : base(randomizers) { }

        public override string GetProgressMessage()
        {
            return "Randomizing Item Contents...";
        }
        public override string GetID()
        {
            return "Item Contents";
        }

        public override void Load()
        {
            shops = new DataStoreBPShop();
            shops.LoadData(File.ReadAllBytes($"data\\randoShops.bin"));

            bazaars = new DataStoreBPSection<DataStoreBazaar>();
            bazaars.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_057.bin"));
        }
        public override void Randomize(Action<int> progressSetter)
        {
        }

        public override void Save()
        {
            File.WriteAllBytes($"outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_039.bin", shops.Data);
            File.WriteAllBytes($"outdata\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_057.bin", bazaars.Data);
        }
    }
}
