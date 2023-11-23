using CsvHelper;
using CsvHelper.Configuration;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Bartz24.Data;

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
        using (CsvParser csv = new(new StreamReader(file), new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false }))
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
    public static void CopyFromFolder(string target, string from)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(from, "*",
            SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(from, target));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(from, "*.*",
            SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(from, target), true);
        }
    }

    public static void ExtractSubfolderFromArchive(string archiveFile, string outputFolderPath, string subfolderName)
    {
        subfolderName = subfolderName.Replace("\\", "/");
        using (SevenZipArchive archive = SevenZipArchive.Open(archiveFile))
        {
            List<SevenZipArchiveEntry> entries = archive.Entries.Where(entry => entry.Key.StartsWith(subfolderName)).ToList();

            foreach (SevenZipArchiveEntry entry in entries)
            {
                string relativePath = entry.Key.Substring(entry.Key.IndexOf(subfolderName) + subfolderName.Length);
                string entryPath = Path.Combine(outputFolderPath, relativePath.Length > 0 ? relativePath.Substring(1) : relativePath);
                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(entryPath);
                }
                else
                {
                    ExtractionOptions options = new();
                    options.Overwrite = true;
                    entry.WriteToFile(entryPath, options);
                }
            }
        }
    }
}
