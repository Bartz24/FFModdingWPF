using System;
using System.Diagnostics;
using System.IO;

namespace Bartz24.Data
{
    public class Nova
    {
        public static string GetNovaFile(string game, string path, string novaPath, string gamePath)
        {
            string filePath = GetFromBackup(game, path, novaPath);
            if (filePath != null)
                return filePath;
            return GetFromUnpacked(game, path, gamePath);
        }

        private static string GetFromBackup(string game, string path, string novaPath)
        {
            string novaFolder = novaPath.Substring(0, novaPath.LastIndexOf("\\"));
            string gameFolder = "";
            switch (game)
            {
                case "13":
                    gameFolder = "XIII";
                    break;
                case "13-2":
                    gameFolder = "XIII-2";
                    break;
                case "LR":
                    gameFolder = "XIII-LR";
                    break;
            }
            string filePath = $"{novaFolder}\\Backup\\{gameFolder}\\{path}";
            if (File.Exists(filePath))
                return filePath;
            return null;
        }

        private static string GetFromUnpacked(string game, string path, string gamePath)
        {
            string gameFolder = "";
            switch (game)
            {
                case "13":
                    gameFolder = "white_data";
                    break;
                case "13-2":
                    gameFolder = "alba_data";
                    break;
                case "LR":
                    gameFolder = "weiss_data";
                    break;
            }
            string filePath = $"{gamePath}\\{gameFolder}\\{path}";
            if (File.Exists(filePath))
                return filePath;
            return null;
        }

        public static bool IsUnpacked(string game, string fileCheck, string gamePath)
        {
            string gameFolder = "";
            switch (game)
            {
                case "13":
                    gameFolder = "white_data";
                    break;
                case "13-2":
                    gameFolder = "alba_data";
                    break;
                case "LR":
                    gameFolder = "weiss_data";
                    break;
            }
            string filePath = $"{gamePath}\\{gameFolder}\\{fileCheck}";
            return File.Exists(filePath);
        }

        public static void ConvertWDBToDB3(string game, string path, string novaPath)
        {
            string gameIndex = GetGameIndex(game);

            RunCommand(novaPath, $"wdbtodb3 \"{Path.GetFullPath(path)}\" {gameIndex}", false);
        }

        public static void ConvertDB3ToWDB(string path, string novaPath)
        {
            RunCommand(novaPath, $"db3towdb \"{Path.GetFullPath(path)}\"", false);
            File.Delete(path);
        }

        public static void UnpackWPD(string path, string novaPath)
        {
            RunCommand(novaPath, $"unpackwpd \"{Path.GetFullPath(path)}\"", false);
        }

        public static void RepackWPD(string path, string novaPath)
        {
            RunCommand(novaPath, $"repackwpd \"{Path.GetFullPath(path)}\"", false);
            Directory.Delete($"{Path.GetDirectoryName(Path.GetFullPath(path))}\\_{Path.GetFileName(Path.GetFullPath(path))}", true);
        }

        private static string GetGameIndex(string game)
        {
            switch (game)
            {
                case "13":
                    return "1";
                case "13-2":
                    return "2";
                case "LR":
                    return "3";
            }
            return null;
        }

        private static void RunCommand(string novaPath, string command, bool showConsole = true)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd";
                process.StartInfo.Arguments = "/c \"NovaChrysalia.exe " + command + "\"";
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(novaPath);
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.UseShellExecute = true;
                process.Start();

                process.WaitForExit();
            }
        }
    }
}
