using Bartz24.Data;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF13Rando
{
    public static class FF13RandoExtensions
    {
        public static void LoadWDB<T>(this DataStoreWDB<T> dataStoreWDB, string game, string relativePath, bool fromNovaOnly = true) where T : DataStoreWDBEntry, new()
        {
            string outPath = SetupData.OutputFolder + relativePath;
            string path = Nova.GetNovaFile(game, relativePath, SetupData.Paths["Nova"], SetupData.Paths[game], !fromNovaOnly);
            if (fromNovaOnly || File.Exists(path))
            {
                FileHelpers.CopyFile(path, outPath, true);
            }

            dataStoreWDB.Load(game, outPath, SetupData.Paths["Nova"]);
        }
        public static void SaveWDB<T>(this DataStoreWDB<T> dataStoreWDB, string relativePath) where T : DataStoreWDBEntry, new()
        {
            string outPath = SetupData.OutputFolder + relativePath;
            dataStoreWDB.Save(outPath, SetupData.Paths["Nova"]);
        }
        public static void DeleteWDB<T>(this DataStoreWDB<T> dataStoreWDB, string relativePath) where T : DataStoreWDBEntry, new()
        {
            string outPath = SetupData.OutputFolder + relativePath;
            File.Delete(outPath);
        }
    }
}
