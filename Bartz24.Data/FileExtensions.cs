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
    public class FileExtensions
    {

        public static void CopyFile(string source, string dest, bool overwrite = false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dest));
            File.Copy(source, dest, overwrite);
        }
        public static void ReadCSVFile(string file, Action<string[]> readRow, bool hasHeader = false)
        {
            using (CsvParser csv = new CsvParser(new StreamReader(file), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
            {
                while (csv.Read())
                {
                    if (csv.Row > 1 || !hasHeader)
                    {
                        readRow(csv.Record);
                    }
                }
            }
        }
    }
}
