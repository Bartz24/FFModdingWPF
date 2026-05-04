using Bartz24.Data;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Bartz24.FF12;

public class DataStoreLicenseBoard : DataStore
{
    protected readonly byte[] header = new byte[] { 0x6C, 0x69, 0x63, 0x64, 0x18, 0x00, 0x18, 0x00 };
    public ushort[,] Board { get; set; } = new ushort[24, 24];

    public DataStoreLicenseBoard()
    {
        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                Board[y, x] = 0xFFFF;
            }
        }
    }

    public void LoadData(string[,] boardData, Func<string, int> nameToInt)
    {
        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                if (string.IsNullOrEmpty(boardData[y, x]))
                {
                    Board[y, x] = 0xFFFF;
                }
                else
                {
                    Board[y, x] = (ushort)nameToInt(boardData[y, x]);
                }
            }
        }
    }

    public void LoadData(string csvFilePath, Func<string, int> nameToInt)
    {
        // Read the 2D grid of license names and map the names to the license IDs
        using (CsvParser csv = new(new StreamReader(csvFilePath), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
        {
            int row = 0;
            while (csv.Read())
            {
                string[] splitRow = csv.Record;
                for (int col = 0; col < 24; col++)
                {
                    if (splitRow.Length <= col || string.IsNullOrEmpty(splitRow[col]))
                    {
                        Board[row, col] = 0xFFFF;
                    }
                    else
                    {
                        Board[row, col] = (ushort)nameToInt(splitRow[col]);
                    }
                }

                row++;
            }
        }
    }

    public override byte[] Data
    {
        get
        {
            byte[] boardData = new byte[24 * 24 * 2];
            for (int x = 0; x < 24; x++)
            {
                for (int y = 0; y < 24; y++)
                {
                    boardData.SetUShort((y * 24 * 2) + (x * 2), Board[y, x]);
                }
            }

            return header.Concat(boardData);
        }
    }

    public override int GetDefaultLength()
    {
        return -1;
    }

    public (int x, int y)? GetCoords(ushort licenseID)
    {
        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                if (Board[y, x] == licenseID)
                {
                    return (x, y);
                }
            }
        }

        return null;
    }

    public (int x, int y)? GetCoordsFromEssentials(ushort licenseID)
    {
        int essentialsID = 31;

        var essentialsCoords = GetCoords((ushort)essentialsID);
        if (essentialsCoords == null)
        {
            return null;
        }

        var licenseCoords = GetCoords(licenseID);
        if (licenseCoords == null)
        {
            return null;
        }

        // Find the offset from the essentials license
        int offsetX = licenseCoords.Value.x - essentialsCoords.Value.x;
        int offsetY = licenseCoords.Value.y - essentialsCoords.Value.y;

        return (offsetX, offsetY);
    }
}
