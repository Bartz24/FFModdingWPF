using System.Diagnostics;
using System.IO;

namespace Bartz24.FF12;

public class Tools
{
    private static string TextPath => "data\\tools\\ff12-text.exe";
    private static string EbpUnpackPath => "data\\tools\\ff12-ebpunpack.exe";
    private static string EbpPackPath => "data\\tools\\ff12-ebppack.exe";
    public static void ConvertBinToTxt(string path)
    {
        RunCommand(TextPath, $"-u \"{Path.GetFullPath(path)}\"");
    }

    public static void ConvertTxtToBin(string path)
    {
        RunCommand(TextPath, $"-p \"{Path.GetFullPath(path)}\"");
        File.Delete(path);
        File.Move(path + ".bin", path.Substring(0, path.Length - 4), true);
    }

    public static void EbpUnpack(string path)
    {
        RunCommand(EbpUnpackPath, $"\"{Path.GetFullPath(path)}\"");
    }

    public static void EbpPack(string path)
    {
        RunCommand(EbpPackPath, $"\"{Path.GetFullPath(path)}\"");
        File.Move(path + "_rec", path, true);
    }

    public static string[] ReadEbpText(string path)
    {
        string folder = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path) + "_unc");
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, true);
        }

        EbpUnpack(path);
        string section2 = Path.Combine(folder, "section_002.bin");
        ConvertBinToTxt(section2);

        string[] text = File.ReadAllLines(section2 + ".txt");

        Directory.Delete(folder, true);

        return text;
    }

    public static void WriteEbpText(string path, string[] text)
    {
        EbpUnpack(path);
        string folder = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path) + "_unc");
        string section2 = Path.Combine(folder, "section_002.bin");

        File.WriteAllLines(section2 + ".txt", text);
        ConvertTxtToBin(section2 + ".txt");
        EbpPack(path);

        Directory.Delete(folder, true);
    }

    private static void RunCommand(string cmdPath, string command)
    {
        using (Process process = new())
        {
            process.StartInfo.FileName = "cmd";
            process.StartInfo.Arguments = "/c \"" + Path.GetFileName(cmdPath) + " " + command + "\"";
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(cmdPath);
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = true;
            process.Start();

            process.WaitForExit();
        }
    }
}
