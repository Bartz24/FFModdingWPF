using Bartz24.Data;
using Bartz24.FF13_2_LR;
using Bartz24.FF13Series;
using Bartz24.RandoWPF;
using System.IO;

namespace FF13_2Rando;

public static class FF13_2RandoExtensions
{
    public static void LoadDB3<T>(this DataStoreWDB<T> dataStoreWDB, SeedGenerator generator, string game, string relativePath, bool fromNovaOnly = true) where T : DataStoreWDBEntry, new()
    {
        string outPath = generator.DataOutFolder + relativePath;
        string path = Nova.GetNovaFile(game, relativePath, SetupData.Paths["Nova"], SetupData.Paths[game], !fromNovaOnly);
        if (fromNovaOnly || File.Exists(path))
        {
            FileHelpers.CopyFile(path, outPath, true);
        }

        dataStoreWDB.Load(game, outPath, SetupData.Paths["Nova"]);
    }
    public static void SaveDB3<T>(this DataStoreWDB<T> dataStoreWDB, SeedGenerator generator, string relativePath) where T : DataStoreWDBEntry, new()
    {
        string outPath = generator.DataOutFolder + relativePath;
        dataStoreWDB.Save("13-2", outPath, SetupData.Paths["Nova"]);
    }
    public static void DeleteDB3<T>(this DataStoreWDB<T> dataStoreWDB, SeedGenerator generator, string relativePath) where T : DataStoreWDBEntry, new()
    {
        string outPath = generator.DataOutFolder + relativePath;
        File.Delete(outPath);
    }

    public static byte[] LoadFile(SeedGenerator generator, string game, string relativePath, bool fromNovaOnly = true)
    {
        string outPath = generator.DataOutFolder + relativePath;
        string path = Nova.GetNovaFile(game, relativePath, SetupData.Paths["Nova"], SetupData.Paths[game], !fromNovaOnly);
        if (fromNovaOnly || File.Exists(path))
        {
            FileHelpers.CopyFile(path, outPath, true);
        }
        return File.ReadAllBytes(outPath);
    }

    public static void SaveFile(SeedGenerator generator, string relativePath, byte[] data)
    {
        string outPath = generator.DataOutFolder + relativePath;
        File.WriteAllBytes(outPath, data);
    }
}
