using Bartz24.FF13;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingUnitTests;
internal class DebugMethods
{
    /*
    [TestMethod()]
    public void ValidateOrdinalOrder()
    {
        string path = @"G:\Steam\steamapps\common\FINAL FANTASY XIII\white_data";

        // create log txt file and clear it
        string logPath = Path.Combine("log.txt");
        File.WriteAllText(logPath, "");

        Dictionary<string, int> failuresByExtension = new();

        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            // Check if the first 4 bytes are 0x57 050 0x44 0x00 using a binary stream
            using (BinaryReader reader = new(File.OpenRead(file)))
            {
                byte[] header = reader.ReadBytes(4);
                if (header.Length < 4 || header[0] != 0x57 || header[1] != 0x50 || header[2] != 0x44 || header[3] != 0x00)
                {
                    continue;
                }
            }

            // get the relative path to the "path" variable
            string relativePath = Path.GetRelativePath(path, file);

            DataStoreWPD dataStoreWPD = new();

            bool noErrors = true;

            dataStoreWPD.Load(file);
            List<string> keys = dataStoreWPD.Keys;
            // Get the keys in ordinal order
            List<string> keysOrdinal = keys.OrderBy(s => s, StringComparer.Ordinal).ToList();
            // Validate the order is the same and log any differences
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i] != keysOrdinal[i])
                {
                    File.AppendAllText(logPath, $"[ERROR] {relativePath}: {keys[i]} - {keysOrdinal[i]}\n");
                    noErrors = false;
                }
            }

            if (noErrors)
            {
                File.AppendAllText(logPath, $"[INFO]  {relativePath}: Uses ordinal/ASCII order!\n");
            }
            else if (failuresByExtension.ContainsKey(Path.GetExtension(file)))
            {
                failuresByExtension[Path.GetExtension(file)] += 1;
            }
            else
            {
                failuresByExtension.Add(Path.GetExtension(file), 1);
            }
        }

        File.AppendAllText(logPath, $"\n\n[INFO]  Failures by extension:\n");
        foreach (KeyValuePair<string, int> pair in failuresByExtension)
        {
            File.AppendAllText(logPath, $"[INFO]  {pair.Key}: {pair.Value}\n");
        }
    }*/
}
