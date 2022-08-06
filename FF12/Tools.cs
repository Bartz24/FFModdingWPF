using System.Diagnostics;
using System.IO;

namespace Bartz24.Data
{
    public class Tools
    {
        private static string TextPath { get => "data\\tools\\ff12-text.exe"; }
        public static void ExtractBINToTXT(string path)
        {
            RunCommand(TextPath, $"-u \"{Path.GetFullPath(path)}\"");
        }

        public static void ConvertTXTToBIN(string path)
        {
            RunCommand(TextPath, $"-p \"{Path.GetFullPath(path)}\"");
            File.Delete(path);
            File.Move(path + ".bin", path.Substring(0, path.Length - 4), true);
        }

        private static void RunCommand(string cmdPath, string command)
        {
            using (Process process = new Process())
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
}
