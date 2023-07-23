using System.IO;

namespace Bartz24.RandoWPF;

public class RandoHelpers
{
    public static void UpdateSeedInFile(string file, string seed)
    {
        string text = File.ReadAllText(file);
        text = text.Replace("_SEED_", seed);
        File.WriteAllText(file, text);
    }

    public static void SaveSeedJSON(string file)
    {
        int seed = RandomNum.GetIntSeed(SetupData.Seed);
        string output = RandoFlags.Serialize(seed.ToString(), SetupData.Version);
        File.WriteAllText(file, output);
    }
}
