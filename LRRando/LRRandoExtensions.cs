using Bartz24.Data;
using Bartz24.FF13_2_LR;
using Bartz24.FF13Series;
using Bartz24.RandoWPF;
using System.IO;

namespace LRRando;

public static class LRRandoExtensions
{
    public static void LoadWDB<T>(this DataStoreWDB<T> dataStoreWDB, SeedGenerator generator, string game, string relativePath, bool fromNovaOnly = true) where T : DataStoreWDBEntry, new()
    {
        string outPath = generator.DataOutFolder + relativePath;
        string path = Nova.GetNovaFile(game, relativePath, SetupData.Paths["Nova"], SetupData.Paths[game], !fromNovaOnly);
        if (fromNovaOnly || File.Exists(path))
        {
            FileHelpers.CopyFile(path, outPath, true);
        }

        dataStoreWDB.Load(game, outPath, SetupData.Paths["Nova"]);
    }
    public static void SaveWDB<T>(this DataStoreWDB<T> dataStoreWDB, SeedGenerator generator, string relativePath) where T : DataStoreWDBEntry, new()
    {
        string outPath = generator.DataOutFolder + relativePath;
        dataStoreWDB.Save("LR", outPath, SetupData.Paths["Nova"]);
    }
    public static void DeleteWDB<T>(this DataStoreWDB<T> dataStoreWDB, SeedGenerator generator, string relativePath) where T : DataStoreWDBEntry, new()
    {
        string outPath = generator.DataOutFolder + relativePath;
        File.Delete(outPath);
    }

    // ItemLocation extensions
    public static bool IsEPAbility(this ItemLocation l)
    {
        return l.GetItem(false).Value.Item.StartsWith("ti") || l.GetItem(false).Value.Item == "at900_00";
    }

    public static bool IsPilgrimKeyItem(this ItemLocation l)
    {
        return l.GetItem(false).Value.Item == "key_d_key";
    }

    public static bool IsLibraNote(this ItemLocation l)
    {
        return l.GetItem(false).Value.Item.StartsWith("libra");
    }

    public static bool IsKeyItem(this ItemLocation l)
    {
        (string Item, int Amount)? item = l.GetItem(false);
        return item != null && LRFlags.Items.KeyItems.DictValues.Forward.Contains(item?.Item);
    }
}
