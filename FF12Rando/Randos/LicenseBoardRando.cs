using Bartz24.Data;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using FF12Rando;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando;

public class LicenseBoardRando : Randomizer
{
    private DataStoreLicenseBoard[] boards = new DataStoreLicenseBoard[12];
    private readonly Dictionary<string, DataStoreLicenseBoard> leftSplitBoards = new();
    private readonly Dictionary<string, DataStoreLicenseBoard> rightSplitBoards = new();
    private int[] startingBoards = new int[6];
    private readonly string[] LeftBoardNames = { "Astrologer", "Dark Bishop", "Elementalist", "Enchanter", "Gambler", "Innkeeper", "Loremaster", "Nightshade", "Red Mage", "Shaman", "Sorceror Supreme", "White Mage" };
    private readonly string[] RightBoardNames = { "Black Belt", "Brawler", "Demolitionist", "Gladiator", "Hunter", "Ninja", "Ravager", "Rogue", "Samurai", "Valkyrie", "Viking", "Weaponmaster" };
    private readonly string[] LeftBoardShort = { "AST", "DBP", "ELE", "ENC", "GMB", "INN", "LOR", "NSH", "RDM", "SMN", "SRC", "WHM" };
    private readonly string[] RightBoardShort = { "BLT", "BWR", "DEM", "GLD", "HNT", "NIN", "RAV", "ROG", "SAM", "VAL", "VKG", "WPN" };

    public LicenseBoardRando(RandomizerManager randomizers) : base(randomizers) { }

    public override void Load()
    {
        Randomizers.SetUIProgress("Loading License Board Data...", 0, -1);
        boards = Enumerable.Range(0, 12).Select(i =>
        {
            DataStoreLicenseBoard board = new();
            board.LoadData(File.ReadAllBytes($"data\\boards\\split\\center.bin"));
            return board;
        }).ToArray();

        Directory.GetFiles("data\\boards\\split\\left", "*.bin").ForEach(s =>
        {
            string fileName = Path.GetFileName(s);
            string name = fileName.Substring(0, fileName.LastIndexOf("."));
            DataStoreLicenseBoard board = new();
            board.LoadData(File.ReadAllBytes(s));
            leftSplitBoards.Add(name, board);
        });
        Directory.GetFiles("data\\boards\\split\\right", "*.bin").ForEach(s =>
        {
            string fileName = Path.GetFileName(s);
            string name = fileName.Substring(0, fileName.LastIndexOf("."));
            DataStoreLicenseBoard board = new();
            board.LoadData(File.ReadAllBytes(s));
            rightSplitBoards.Add(name, board);
        });

        TreasureRando treasureRando = Randomizers.Get<TreasureRando>();

        treasureRando.prices[0x77].Price = 0;
        startingBoards = MathHelpers.DecodeNaturalSequence(treasureRando.prices[0x77].Price, 6, 13).Select(l => (int)l).ToArray();
    }
    public override void Randomize()
    {
        Randomizers.SetUIProgress("Randomizing License Board Data...", 0, -1);
        TreasureRando treasureRando = Randomizers.Get<TreasureRando>();
        TextRando textRando = Randomizers.Get<TextRando>();

        if (FF12Flags.Other.LicenseBoards.FlagEnabled)
        {
            FF12Flags.Other.LicenseBoards.SetRand();
            int[] left = Enumerable.Range(0, 12).Shuffle().ToArray();
            int[] right = Enumerable.Range(0, 12).Shuffle().ToArray();
            for (int i = 0; i < 12; i++)
            {
                AddBoard(boards[i], leftSplitBoards.Values.ToList()[left[i]]);
                AddBoard(boards[i], rightSplitBoards.Values.ToList()[right[i]]);

                textRando.TextMenuMessage[104 + i].Text = LeftBoardNames[left[i]] + " - " + RightBoardNames[right[i]] + "\n {0x0f}où Preview License Board";
                textRando.TextMenuCommand[5 + i].Text = LeftBoardShort[left[i]] + "-" + RightBoardShort[right[i]];
            }

            RandomNum.ClearRand();
        }

        if (FF12Flags.Other.StartingBoards.FlagEnabled)
        {
            FF12Flags.Other.StartingBoards.SetRand();
            startingBoards = Enumerable.Range(1, 12).Shuffle().Take(6).ToArray();
            treasureRando.prices[0x77].Price = (uint)MathHelpers.EncodeNaturalSequence(startingBoards.Select(i => (long)i).ToArray(), 13);
            RandomNum.ClearRand();
        }
    }

    private void AddBoard(DataStoreLicenseBoard main, DataStoreLicenseBoard add)
    {
        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                if (add.Board[y, x] != 0xFFFF)
                {
                    main.Board[y, x] = add.Board[y, x];
                }
            }
        }
    }

    public override void Save()
    {
        Randomizers.SetUIProgress("Saving License Board Data...", 0, -1);
        if (FF12Flags.Other.LicenseBoards.FlagEnabled)
        {
            for (int i = 0; i < 12; i++)
            {
                string path = $"outdata\\ps2data\\image\\ff12\\test_battle\\in\\binaryfile\\board_{i + 1}.bin";
                Directory.CreateDirectory($"outdata\\ps2data\\image\\ff12\\test_battle\\in\\binaryfile");
                File.WriteAllBytes(path, boards[i].Data);
            }
        }
        else
        {
            for (int i = 0; i < 12; i++)
            {
                string path = $"outdata\\ps2data\\image\\ff12\\test_battle\\in\\binaryfile\\board_{i + 1}.bin";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
