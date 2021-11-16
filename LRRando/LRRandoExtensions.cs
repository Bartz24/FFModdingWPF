using Bartz24.Data;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando
{
    public static class LRRandoExtensions
    {
        public static void LoadDB3<T>(this DataStoreDB3<T> dataStoreDB3, string game, string relativePath, bool fromNovaOnly = true) where T : DataStoreDB3SubEntry, new()
        {
            string outPath = SetupData.OutputFolder + relativePath;
            string path = Nova.GetNovaFile(game, relativePath, SetupData.Paths["Nova"], SetupData.Paths[game]);
            if (fromNovaOnly || File.Exists(path))
            {
                FileExtensions.CopyFile(path, outPath, true);
            }

            dataStoreDB3.Load(game, outPath, SetupData.Paths["Nova"]);
        }
        public static void SaveDB3<T>(this DataStoreDB3<T> dataStoreDB3, string relativePath) where T : DataStoreDB3SubEntry, new()
        {
            string outPath = SetupData.OutputFolder + relativePath;
            dataStoreDB3.Save(outPath, SetupData.Paths["Nova"]);
        }
        public static void DeleteDB3<T>(this DataStoreDB3<T> dataStoreDB3, string relativePath) where T : DataStoreDB3SubEntry, new()
        {
            string outPath = SetupData.OutputFolder + relativePath;
            File.Delete(outPath);
        }
    }
}
