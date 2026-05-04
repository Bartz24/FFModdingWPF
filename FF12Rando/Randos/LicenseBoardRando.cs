using Bartz24.Data;
using Bartz24.FF12;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando;

public class LicenseBoardRando : Randomizer
{
    public DataStoreLicenseBoard[] boards = new DataStoreLicenseBoard[12];
    private int[] startingBoards = new int[6];

    private (int left, int right)[] splitBoardIndices = new (int left, int right)[12];

    private readonly string[] LeftBoardIds = { "astrologer", "darkbishop", "elementalist", "enchanter", "gambler", "chemist", "loremaster", "nightshade", "redmage", "shaman", "sorcerorsupreme", "whitemage" };
    private readonly string[] RightBoardIds = { "blackbelt", "brawler", "demolitionist", "gladiator", "hunter", "ninja", "ravager", "rogue", "samurai", "valkyrie", "viking", "weaponmaster" };

    private readonly string[] LeftBoardNames = { "Astrologer", "Dark Bishop", "Elementalist", "Enchanter", "Gambler", "Chemist", "Loremaster", "Nightshade", "Red Mage", "Shaman", "Sorceror Supreme", "White Mage" };
    private readonly string[] RightBoardNames = { "Black Belt", "Brawler", "Demolitionist", "Gladiator", "Hunter", "Ninja", "Ravager", "Rogue", "Samurai", "Valkyrie", "Viking", "Weaponmaster" };

    private readonly string[] LeftBoardShort = { "AST", "DBP", "ELE", "ENC", "GMB", "CHM", "LOR", "NSH", "RDM", "SMN", "SRC", "WHM" };
    private readonly string[] RightBoardShort = { "BLT", "BWR", "DEM", "GLD", "HNT", "NIN", "RAV", "ROG", "SAM", "VAL", "VKG", "WPN" };

    private Dictionary<string, DataStoreLicenseBoard> vanillaBoards = new();
    private readonly string[] VanillaBoardIds = { "whitemage", "uhlan", "machinist", "redbattlemage", "knight", "monk", "timebattlemage", "foebreaker", "archer", "blackmage", "bushi", "shikari" };
    private readonly string[] VanillaBoardNames = { "White Mage", "Uhlan", "Machinist", "Red Battlemage", "Knight", "Monk", "Time Battlemage", "Foebreaker", "Archer", "Black Mage", "Bushi", "Shikari" };

    private readonly string[] VanillaBoardShort = { "WHM", "UHL", "MCH", "RDM", "KNT", "MNK", "TIM", "FBK", "ARC", "BLK", "BUS", "SHI" };

    public LicenseBoardRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        RandoUI.SetUIProgressIndeterminate("Loading License Board Data...");
        for (int i = 0; i < 12; i++)
        {
            boards[i] = new();
        }

        LicenseRando licenseRando = Generator.Get<LicenseRando>();

        if (FF12Flags.Licenses.BoardType.SelectedValue == FF12Flags.Licenses.BoardTypeVanilla)
        {
            Directory.GetFiles("data\\licenses\\vanilla\\boards", "*.csv").ForEach(s =>
            {
                string fileName = Path.GetFileName(s);
                string name = fileName.Substring(0, fileName.LastIndexOf("."));
                DataStoreLicenseBoard board = new();
                board.LoadData(s, lName => licenseRando.licenseData.Values.First(l => l.Name == lName).IntID);
                vanillaBoards.Add(name, board);
            });
        }

        TreasureRando treasureRando = Generator.Get<TreasureRando>();

        treasureRando.prices[0x77].Price = 0;
        startingBoards = MathHelpers.DecodeNaturalSequence(treasureRando.prices[0x77].Price, 6, 13).Select(l => (int)l).ToArray();
    }
    public override void Randomize()
    {
        RandoUI.SetUIProgressIndeterminate("Randomizing License Board Data...");
        TreasureRando treasureRando = Generator.Get<TreasureRando>();
        TextRando textRando = Generator.Get<TextRando>();

        FF12Flags.Licenses.LicenseBoardType.SetRand();

        if (FF12Flags.Licenses.BoardType.SelectedValue == FF12Flags.Licenses.BoardTypeVanilla)
        {

            foreach (string id in VanillaBoardIds)
            {
                int i = VanillaBoardIds.ToList().IndexOf(id);
                AddBoard(boards[i], vanillaBoards[id]);
            }
        }
        else if (FF12Flags.Licenses.BoardType.SelectedValue == FF12Flags.Licenses.BoardTypeSplit)
        {
            BuildRandomSplitBoards();
        }

        for (int i = 0; i < boards.Length; i++)
        {
            textRando.TextMenuMessage[104 + i].Text = GetBoardName(i) + "\n {btn:square} Preview License Board";
            textRando.TextMenuCommand[5 + i].Text = $"{GetBoardShort(i)} ({GetBoardLetter(i)})";
        }

        CenterBoards();

        RandomNum.ClearRand();

        if (FF12Flags.Licenses.StartingBoards.FlagEnabled)
        {
            FF12Flags.Licenses.StartingBoards.SetRand();
            startingBoards = Enumerable.Range(1, 12).Shuffle().Take(6).ToArray();
            treasureRando.prices[0x77].Price = (uint)MathHelpers.EncodeNaturalSequence(startingBoards.Select(i => (long)i).ToArray(), 13);
            RandomNum.ClearRand();
        }
    }

    private void BuildRandomSplitBoards()
    {
        // Randomizes lists of indices for each side and then pair them up
        List<int> leftIndices = Enumerable.Range(0, 12).Shuffle();
        List<int> rightIndices = Enumerable.Range(0, 12).Shuffle();

        for (int i = 0; i < 12; i++)
        {
            splitBoardIndices[i] = (leftIndices[i], rightIndices[i]);
        }

        // Go through each board and add the left and right sides with the center
        for (int i = 0; i < 12; i++)
        {
            string[,] left = LoadSplitBoardFromCsv($"data\\licenses\\dual\\boards\\left\\{LeftBoardIds[splitBoardIndices[i].left]}.csv");
            string[,] center = LoadSplitBoardFromCsv($"data\\licenses\\dual\\boards\\center.csv");
            string[,] right = LoadSplitBoardFromCsv($"data\\licenses\\dual\\boards\\right\\{RightBoardIds[splitBoardIndices[i].right]}.csv");

            boards[i] = BuildSplitBoard(left, center, right);
        }
    }

    private string[,] LoadSplitBoardFromCsv(string path)
    {
        string[,] board = new string[24, 24];
        int curRow = 0;
        FileHelpers.ReadCSVFile(path, row =>
        {
            for (int col = 0; col < 24; col++)
            {
                if (row.Length <= col || string.IsNullOrEmpty(row[col]))
                {
                    board[curRow, col] = "";
                }
                else
                {
                    board[curRow, col] = row[col];
                }
            }

            curRow++;
        }, FileHelpers.CSVFileHeader.NoHeader);

        return board;
    }

    private DataStoreLicenseBoard BuildSplitBoard(string[,] left, string[,] center, string[,] right)
    {
        // Start by placing the center board in the middle of the 24x24 grid
        string[,] board = new string[24, 24];
        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                board[y, x] = "";
            }
        }

        int minX = 24;
        int minY = 24;
        int maxX = 0;
        int maxY = 0;

        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                if (!string.IsNullOrEmpty(center[y, x]))
                {
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }
        }

        int offsetX = (24 - (maxX - minX + 1)) / 2 - minX;
        int offsetY = (24 - (maxY - minY + 1)) / 2 - minY;
        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                if (x + offsetX >= 0 && x + offsetX < 24 && y + offsetY >= 0 && y + offsetY < 24)
                {
                    board[y + offsetY, x + offsetX] = center[y, x];
                }
            }
        }

        // Place the left board by ligning up the "ALIGN_TOP" cell in the left board on the "ALIGN_TOP_LEFT" cell in the board
        // Find the "ALIGN_TOP" cell in the left board
        (int x, int y) leftAlignTop = (0, 0);
        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                if (left[y, x] == "ALIGN_TOP")
                {
                    leftAlignTop = (x, y);
                    break;
                }
            }
        }

        // Find the "ALIGN_TOP_LEFT" cell in the board and shift one to the right
        (int x, int y) boardAlignTopLeft = (0, 0);
        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                if (board[y, x] == "ALIGN_TOP_LEFT")
                {
                    boardAlignTopLeft = (x, y);
                    break;
                }
            }
        }

        boardAlignTopLeft.x++;

        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                if (!string.IsNullOrEmpty(left[y, x]) && !left[y, x].StartsWith("ALIGN_"))
                {
                    (int x, int y) posRelativeToAlignTop = (x - leftAlignTop.x, y - leftAlignTop.y);
                    board[boardAlignTopLeft.y + posRelativeToAlignTop.y, boardAlignTopLeft.x + posRelativeToAlignTop.x] = left[y, x];
                }
            }
        }

        // Place the right board by ligning up the "ALIGN_TOP" cell in the right board on the "ALIGN_TOP_RIGHT" cell in the board
        // Find the "ALIGN_TOP" cell in the right board
        (int x, int y) rightAlignTop = (0, 0);
        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                if (right[y, x] == "ALIGN_TOP")
                {
                    rightAlignTop = (x, y);
                    break;
                }
            }
        }

        // Find the "ALIGN_TOP_RIGHT" cell in the board and shift one to the left
        (int x, int y) boardAlignTopRight = (0, 0);
        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                if (board[y, x] == "ALIGN_TOP_RIGHT")
                {
                    boardAlignTopRight = (x, y);
                    break;
                }
            }
        }

        boardAlignTopRight.x--;

        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                if (x + offsetX >= 0 && x + offsetX < 24 && y + offsetY >= 0 && y + offsetY < 24 && !string.IsNullOrEmpty(right[y, x]) && !right[y, x].StartsWith("ALIGN_"))
                {
                    (int x, int y) posRelativeToAlignTop = (x - rightAlignTop.x, y - rightAlignTop.y);
                    board[boardAlignTopRight.y + posRelativeToAlignTop.y, boardAlignTopRight.x + posRelativeToAlignTop.x] = right[y, x];
                }
            }
        }

        // Clean up the board
        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                // Replace nulls with empty strings
                if (board[y, x] == null)
                {
                    board[y, x] = "";
                }
                // Clear out any cells that start with "ALIGN_"
                else if (board[y, x].StartsWith("ALIGN_"))
                {
                    board[y, x] = "";
                }
            }
        }

        // Convert the board to a DataStoreLicenseBoard
        LicenseRando licenseRando = Generator.Get<LicenseRando>();
        DataStoreLicenseBoard data = new();
        data.LoadData(board, lName => licenseRando.licenseData.Values.First(l => l.Name == lName).IntID);

        return data;
    }

    public string GetBoardName(int i)
    {
        if (FF12Flags.Licenses.BoardType.SelectedValue == FF12Flags.Licenses.BoardTypeSplit)
        {
            return $"{LeftBoardNames[splitBoardIndices[i].left]}-{RightBoardNames[splitBoardIndices[i].right]}";
        }
        else
        {
            return VanillaBoardNames[i];
        }
    }

    public string GetBoardShort(int i)
    {
        if (FF12Flags.Licenses.BoardType.SelectedValue == FF12Flags.Licenses.BoardTypeSplit)
        {
            return $"{LeftBoardShort[splitBoardIndices[i].left]}-{RightBoardShort[splitBoardIndices[i].right]}";
        }
        else
        {
            return VanillaBoardShort[i];
        }
    }

    public string GetBoardLetter(int i)
    {
        // Return A, B, C, etc
        return ((char)(65 + i)).ToString().ToUpper();
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

    private void CenterBoards()
    {
        // Center each board in the middle of the 24x24 grid
        for (int i = 0; i < 12; i++)
        {
            DataStoreLicenseBoard board = boards[i];
            int minX = 24;
            int minY = 24;
            int maxX = 0;
            int maxY = 0;
            for (int x = 0; x < 24; x++)
            {
                for (int y = 0; y < 24; y++)
                {
                    if (board.Board[y, x] != 0xFFFF)
                    {
                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }

            ushort[,] newBoard = new ushort[24, 24];
            // Fill with 0xFFFF
            for (int x = 0; x < 24; x++)
            {
                for (int y = 0; y < 24; y++)
                {
                    newBoard[y, x] = 0xFFFF;
                }
            }

            int offsetX = (24 - (maxX - minX + 1)) / 2 - minX;
            int offsetY = (24 - (maxY - minY + 1)) / 2 - minY;
            for (int x = 0; x < 24; x++)
            {
                for (int y = 0; y < 24; y++)
                {
                    if (x + offsetX >= 0 && x + offsetX < 24 && y + offsetY >= 0 && y + offsetY < 24)
                    {
                        newBoard[y + offsetY, x + offsetX] = board.Board[y, x];
                    }
                }
            }

            board.Board = newBoard;
        }
    }

    public override void Save()
    {
        RandoUI.SetUIProgressIndeterminate("Saving License Board Data...");

        for (int i = 0; i < 12; i++)
        {
            string path = $"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\in\\binaryfile\\board_{i + 1}.bin";
            Directory.CreateDirectory($"{Generator.DataOutFolder}\\image\\ff12\\test_battle\\in\\binaryfile");
            File.WriteAllBytes(path, boards[i].Data);
        }
    }
}
