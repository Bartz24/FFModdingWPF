using Bartz24.Data;
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
        string output = RandoFlags.Serialize(SetupData.Seed.Clean(1000), SetupData.Version);
        File.WriteAllText(file, output);
    }
}
