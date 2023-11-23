using Bartz24.Data;
using Bartz24.FF13_2_LR;
using Bartz24.RandoWPF;
using System.IO;

namespace LRRando;

public static class LRRandoExtensions
{
    public static void LoadDB3<T>(this DataStoreDB3<T> dataStoreDB3, SeedGenerator generator, string game, string relativePath, bool fromNovaOnly = true) where T : DataStoreDB3SubEntry, new()
    {
        string outPath = generator.DataOutFolder + relativePath;
        string path = Nova.GetNovaFile(game, relativePath, SetupData.Paths["Nova"], SetupData.Paths[game], !fromNovaOnly);
        if (fromNovaOnly || File.Exists(path))
        {
            FileHelpers.CopyFile(path, outPath, true);
        }

        dataStoreDB3.Load(game, outPath, SetupData.Paths["Nova"]);
    }
    public static void SaveDB3<T>(this DataStoreDB3<T> dataStoreDB3, SeedGenerator generator, string relativePath) where T : DataStoreDB3SubEntry, new()
    {
        string outPath = generator.DataOutFolder + relativePath;
        dataStoreDB3.Save(outPath, SetupData.Paths["Nova"]);
    }
    public static void DeleteDB3<T>(this DataStoreDB3<T> dataStoreDB3, SeedGenerator generator, string relativePath) where T : DataStoreDB3SubEntry, new()
    {
        string outPath = generator.DataOutFolder + relativePath;
        File.Delete(outPath);
    }
}
