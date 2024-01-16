using CsvHelper;
using CsvHelper.Configuration;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;
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
            archive.ExtractAllEntries();
            using (var reader = archive.ExtractAllEntries())
            {
                while (reader.MoveToNextEntry())
                {
                    if (reader.Entry.Key.StartsWith(subfolderName))
                    {
                        string relativePath = reader.Entry.Key.Substring(reader.Entry.Key.IndexOf(subfolderName) + subfolderName.Length);
                        string entryPath = Path.Combine(outputFolderPath, relativePath.Length > 0 ? relativePath.Substring(1) : relativePath);
                        if (reader.Entry.IsDirectory)
                        {
                            Directory.CreateDirectory(Path.Combine(outputFolderPath, reader.Entry.Key));
                        }
                        else
                        {
                            ExtractionOptions options = new();
                            options.Overwrite = true;
                            reader.WriteEntryToFile(entryPath, options);
                        }
                    }
                }
            }
        }
    }

    public static bool RemoveFilesAndFolders(string folderPath, List<string> denyList)
    {
        if (!Directory.Exists(folderPath))
        {
            return false;
        }

        bool filesRemaining = false;
        try
        {
            // Process each file in the folder
            foreach (string filePath in Directory.GetFiles(folderPath))
            {
                // Check if the file path is not in the denylist
                if (!denyList.Contains(filePath))
                {
                    File.Delete(filePath);
                }
                else
                {
                    filesRemaining = true;
                }
            }

            // Process each subfolder in the folder
            foreach (string subfolderPath in Directory.GetDirectories(folderPath))
            {
                // Check if the folder path is not in the denylist
                if (!denyList.Contains(subfolderPath))
                {
                    // Recursively call the method for each subfolder
                    bool subfilesRemaining = RemoveFilesAndFolders(subfolderPath, denyList);
                    if (!subfilesRemaining)
                    {
                        Directory.Delete(subfolderPath);
                    }
                    else
                    {
                        filesRemaining = true;
                    }
                }
                else
                {
                    filesRemaining = true;
                }
            }
        }
        catch (Exception)
        {
            // Handle exceptions if needed
        }

        return filesRemaining;
    }
}
