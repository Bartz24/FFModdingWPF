using Bartz24.Data;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando
{
    public class LicenseBoardRando : Randomizer
    {
        DataStoreLicenseBoard[] boards = new DataStoreLicenseBoard[12];
        Dictionary<string, DataStoreLicenseBoard> leftSplitBoards = new Dictionary<string, DataStoreLicenseBoard>();
        Dictionary<string, DataStoreLicenseBoard> rightSplitBoards = new Dictionary<string, DataStoreLicenseBoard>();
        int[] startingBoards = new int[6];

        string[] LeftBoardNames = new string[] { "Astrologer", "Dark Bishop", "Elementalist", "Enchanter", "Gambler", "Innkeeper", "Loremaster", "Nightshade", "Red Mage", "Shaman", "Sorceror Supreme", "White Mage" };
        string[] RightBoardNames = new string[] { "Black Belt", "Brawler", "Demolitionist", "Gladiator", "Hunter", "Ninja", "Ravager", "Rogue", "Samurai", "Valkyrie", "Viking", "Weaponmaster" };
        string[] LeftBoardShort = new string[] { "AST", "DBP", "ELE", "ENC", "GMB", "INN", "LOR", "NSH", "RDM", "SMN", "SRC", "WHM" };
        string[] RightBoardShort = new string[] { "BLT", "BWR", "DEM", "GLD", "HNT", "NIN", "RAV", "ROG", "SAM", "VAL", "VKG", "WPN" };

        public LicenseBoardRando(RandomizerManager randomizers) : base(randomizers) { }

        public override string GetID()
        {
            return "License Boards";
        }

        public override void Load()
        {
            boards = Enumerable.Range(0, 12).Select(i =>
            {
                DataStoreLicenseBoard board = new DataStoreLicenseBoard();
                board.LoadData(File.ReadAllBytes($"data\\boards\\split\\center.bin"));
                return board;
            }).ToArray();

            Directory.GetFiles("data\\boards\\split\\left", "*.bin").ForEach(s =>
            {
                string fileName = Path.GetFileName(s);
                string name = fileName.Substring(0, fileName.LastIndexOf("."));
                DataStoreLicenseBoard board = new DataStoreLicenseBoard();
                board.LoadData(File.ReadAllBytes(s));
                leftSplitBoards.Add(name, board);
            });
            Directory.GetFiles("data\\boards\\split\\right", "*.bin").ForEach(s =>
            {
                string fileName = Path.GetFileName(s);
                string name = fileName.Substring(0, fileName.LastIndexOf("."));
                DataStoreLicenseBoard board = new DataStoreLicenseBoard();
                board.LoadData(File.ReadAllBytes(s));
                rightSplitBoards.Add(name, board);
            });

            TreasureRando treasureRando = Randomizers.Get<TreasureRando>("Treasures");

            treasureRando.prices[0x77].Price = 0;
            startingBoards = MathExtensions.DecodeNaturalSequence(treasureRando.prices[0x77].Price, 6, 13).Select(l => (int)l).ToArray();
        }
        public override void Randomize(Action<int> progressSetter)
        {
            TreasureRando treasureRando = Randomizers.Get<TreasureRando>("Treasures");
            TextRando textRando = Randomizers.Get<TextRando>("Text");

            if (FF12Flags.Other.LicenseBoards.FlagEnabled)
            {
                FF12Flags.Other.LicenseBoards.SetRand();
                int[] left = Enumerable.Range(0, 12).ToList().Shuffle().ToArray();
                int[] right = Enumerable.Range(0, 12).ToList().Shuffle().ToArray();
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
                startingBoards = Enumerable.Range(1, 12).ToList().Shuffle().Take(6).ToArray();
                treasureRando.prices[0x77].Price = (uint)MathExtensions.EncodeNaturalSequence(startingBoards.Select(i => (long)i).ToArray(), 13);
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
                        File.Delete(path);
                }
            }
        }
    }
}
