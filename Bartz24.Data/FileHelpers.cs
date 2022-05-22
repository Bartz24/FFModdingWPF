using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.Data
{
    public class FileHelpers
    {

        public static void CopyFile(string source, string dest, bool overwrite = false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dest));
            File.Copy(source, dest, overwrite);
        }

        public enum CSVFileHeader
        {
            HasHeader,
            NoHeader
        }

        public static void ReadCSVFile(string file, Action<string[]> readRow, CSVFileHeader fileHeader)
        {
            using (CsvParser csv = new CsvParser(new StreamReader(file), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
            {
                while (csv.Read())
                {
                    if (csv.Row > 1 || fileHeader == CSVFileHeader.NoHeader)
                    {
                        readRow(csv.Record);
                    }
                }
            }
        }
    }
}
