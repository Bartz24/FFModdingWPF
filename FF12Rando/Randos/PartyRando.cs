using Bartz24.Data;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using FF12Rando;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando;

public class PartyRando : Randomizer
{
    public DataStoreBPSection<DataStorePartyMember> party = new();
    public DataStoreBPSection<DataStorePartyMember> partyOrig = new();

    public int[] Characters = new int[6];
    public string[] CharacterMapping = { "Vaan", "Ashe", "Fran", "Balthier", "Basch", "Penelo" };

    public PartyRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        Generator.SetUIProgress("Loading Party Data...", 0, -1);
        party = new DataStoreBPSection<DataStorePartyMember>();
        party.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_016.bin"));
        party.DataList.ForEach(c =>
        {
            ReplaceGambits(c);
        });

        partyOrig = new DataStoreBPSection<DataStorePartyMember>();
        partyOrig.LoadData(File.ReadAllBytes($"data\\ps2data\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_016.bin"));
        partyOrig.DataList.ForEach(c =>
        {
            ReplaceGambits(c);
        });

        static void ReplaceGambits(DataStorePartyMember c)
        {
            List<ushort> itemIDs = c.ItemIDs;
            List<byte> itemAmounts = c.ItemAmounts;

            for (int i = 0; i < itemIDs.Count; i++)
            {
                if (itemIDs[i] is >= 0x6000 and < 0x7000)
                {
                    itemIDs[i] = 0xFFFF;
                    itemAmounts[i] = 0;
                }
            }

            itemIDs = itemIDs.OrderBy(i => i == 0xFFFF).ToList();
            itemAmounts = Enumerable.Range(0, itemAmounts.Count).OrderBy(i => itemIDs[i] == 0xFFFF).Select(i => itemAmounts[i]).ToList();

            c.ItemIDs = itemIDs;
            c.ItemAmounts = itemAmounts;
        }
    }
    public override void Randomize()
    {
        Generator.SetUIProgress("Randomizing Party Data...", 0, -1);
        if (FF12Flags.Other.Party.FlagEnabled)
        {
            FF12Flags.Other.Party.SetRand();
            Characters = Characters.Shuffle().ToArray();
            TreasureRando treasureRando = Generator.Get<TreasureRando>();
            treasureRando.prices[0x76].Price = (uint)MathHelpers.EncodeNaturalSequence(Characters.Select(i => (long)i).ToArray(), 6);
            RandomNum.ClearRand();

            party[Characters[0]].Level = 1;
            party[Characters[0]].LP = 6;
            party[Characters[0]].RecalculateLevelLP = DataStorePartyMember.RecalculateLevelLPType.Never;

            for (int i = 1; i < 6; i++)
            {
                party[Characters[i]].Level = 0;
                party[Characters[i]].RecalculateLevelLP = DataStorePartyMember.RecalculateLevelLPType.Once;
                party[Characters[i]].LP = 15;
            }
        }
    }

    public override void Save()
    {
        if (FF12Flags.Items.StartingTpStones.FlagEnabled)
        {
            DataStorePartyMember c = party[Characters[0]];
            List<ushort> itemIDs = c.ItemIDs;
            List<byte> itemAmounts = c.ItemAmounts;
            // Use the last slot for TP stones
            itemIDs[9] = 0x2000;
            itemAmounts[9] = (byte)FF12Flags.Items.TpStoneCount.Value;

            c.ItemIDs = itemIDs;
            c.ItemAmounts = itemAmounts;
        }

        Generator.SetUIProgress("Saving Party Data...", 0, -1);
        File.WriteAllBytes($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\us\\binaryfile\\battle_pack.bin.dir\\section_016.bin", party.Data);
    }
}
