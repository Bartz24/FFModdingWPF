using Bartz24.Data;
using Bartz24.FF13;
using Bartz24.RandoWPF;
using System.IO;

namespace FF13Rando;

public static class FF13RandoExtensions
{
    public static void LoadWDB<T>(this DataStoreWDB<T> dataStoreWDB, SeedGenerator generator, string game, string relativePath, bool fromNovaOnly = true) where T : DataStoreWDBEntry, new()
    {
        string outPath = generator.DataOutFolder + relativePath;
        string path = Nova.GetNovaFile(game, relativePath, SetupData.Paths["Nova"], SetupData.Paths[game], !fromNovaOnly);
        if (fromNovaOnly || File.Exists(path))
        {
            FileHelpers.CopyFile(path, outPath, true);
        }

        dataStoreWDB.Load(path);
    }
    public static void SaveWDB<T>(this DataStoreWDB<T> dataStoreWDB, SeedGenerator generator, string relativePath) where T : DataStoreWDBEntry, new()
    {
        string outPath = generator.DataOutFolder + relativePath;
        dataStoreWDB.Save(outPath);
    }
    public static void DeleteWDB<T>(this DataStoreWDB<T> dataStoreWDB, SeedGenerator generator, string relativePath) where T : DataStoreWDBEntry, new()
    {
        string outPath = generator.DataOutFolder + relativePath;
        File.Delete(outPath);
    }
}
