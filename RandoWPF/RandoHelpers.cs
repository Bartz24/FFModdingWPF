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
}
