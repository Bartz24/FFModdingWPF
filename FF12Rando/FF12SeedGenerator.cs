using Bartz24.Data;
using Bartz24.RandoWPF;
using FF12Rando.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FF12Rando;
public class FF12SeedGenerator : SeedGenerator
{
    private static List<string> ToolPaths
    {
        get => new()
        {
            "data\\tools\\ff12-text.exe",
            "data\\tools\\ff12-ebppack.exe",
            "data\\tools\\ff12-ebpunpack.exe"
        };
    }
    private static List<string> FileLoaderPaths
    {
        get
        {
            if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
            {
                throw new RandoException("Missing steam path", "Invalid path");
            }

            return new()
            {
                Path.Combine(SetupData.Paths["12"], "x64\\ff12-trampoline.dll"),
                Path.Combine(SetupData.Paths["12"], "x64\\vcruntime140_1.dll"),
                Path.Combine(SetupData.Paths["12"], "x64\\modules\\ff12-file-loader.dll"),
                Path.Combine(SetupData.Paths["12"], "x64\\modules\\config\\ff12-file-loader.ini")
            };
        }
    }
    private static List<string> LuaLoaderPaths
    {
        get => new()
        {
            Path.Combine(SetupData.Paths["12"], "x64\\modules\\ff12-lua-loader.dll")
        };
    }
    private static List<string> DescriptivePaths
    {
        get
        {
            if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
            {
                throw new RandoException("Missing steam path", "Invalid path");
            }

            return new() 
            {
                Path.Combine(SetupData.Paths["12"], "x64\\scripts\\TheInsurgentsDescriptiveInventory.lua"),
                Path.Combine(SetupData.Paths["12"], "x64\\scripts\\TheInsurgentsDescriptiveInventory\\helpers.lua"),
                Path.Combine(SetupData.Paths["12"], "x64\\scripts\\config\\TheInsurgentsDescriptiveInventoryConfig\\us.lua")
            };
        }
    }

    private static List<string> ManifestoRequiredPathsRandoInstall
    {
        get
        {
            if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
            {
                throw new RandoException("Missing steam path", "Invalid path");
            }

            return new()
            {
                Path.Combine(SetupData.Paths["12"], "rando\\ps2data\\image\\ff12\\in\\common\\pc_skillmotion.bin"),
                Path.Combine(SetupData.Paths["12"], "x64\\scripts\\TheInsurgentsManifesto.lua")
            };
        }
    }

    private static List<string> ManifestoRequiredPathsVortexInstall
    {
        get
        {
            if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
            {
                throw new RandoException("Missing steam path", "Invalid path");
            }

            return new()
            {
                Path.Combine(SetupData.Paths["12"], "mods\\deploy\\ps2data\\image\\ff12\\in\\common\\pc_skillmotion.bin"),
                Path.Combine(SetupData.Paths["12"], "x64\\scripts\\TheInsurgentsManifesto.lua")
            };
        }
    }

    public FF12SeedGenerator() : base()
    {
        Randomizers = new()
        {
            new PartyRando(this),
            new TreasureRando(this),
            new EquipRando(this),
            new LicenseBoardRando(this),
            new EnemyRando(this),
            new ShopRando(this),
            new MusicRando(this),
            new TextRando(this)
        };

        OutFolder = Path.Combine(SetupData.Paths["12"], "rando");
        DataOutFolder = Path.Combine(OutFolder, "ps2data");

        PackPrefixName = "FF12Rando";
        DocsDisplayName = "FF12 Randomizer";
        DocsOutFolder = "docs";

        AeropassItemReq.Init();
    }

    protected override void PrepareData()
    {
        if (!ToolsInstalled())
        {
            throw new RandoException("Text and script tools are not properly installed. Download and install them on 1. Setup.", "Tools missing.");
        }

        if(!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            throw new RandoException("Missing steam path", "Invalid path");
        }

        if (!FileLoaderInstalled())
        {
            throw new RandoException("External File Loader is not properly installed. Download and install them on 1. Setup.", "External File Loader missing.");
        }

        if (!LuaLoaderInstalled())
        {
            throw new RandoException("Lua Loader is not properly installed. Download and install them on 1. Setup.", "Lua missing.");
        }

        if (ManifestoInstalled() == ManifestoInstallType.Missing)
        {
            throw new RandoException("The Insurgent's Manifesto is not properly installed. Download and install them on 1. Setup.", "Insurgent's Manifesto missing.");
        }

        if (FF12Flags.Items.Treasures.FlagEnabled && FF12Flags.Items.WritGoals.SelectedIndices.Count == 0)
        {
            throw new RandoException("Item location randomization is turned on but there is no goal selected. Select at least 1 Bahamut unlock condition.", "No goal selected.");
        }

        if (Directory.Exists(OutFolder))
        {
            List<string> denyList = new (){
                Path.Combine(SetupData.Paths["12"], "rando\\ps2data\\image\\ff12\\in\\common\\pc_skillmotion.bin"),
                Path.Combine(SetupData.Paths["12"], "rando\\ps2data\\obj_finish\\in\\chara"),
            };
            FileHelpers.RemoveFilesAndFolders(OutFolder, denyList);
        }

        Directory.CreateDirectory(OutFolder);
        FileHelpers.CopyFromFolder(Path.Combine(OutFolder, "ps2data"), "data\\ps2data");

        SetupData.WPDTracking.Clear();

        UpdateLoaderConfig();
        RemoveAndMoveLuaScripts();
        CopyLuaScripts();

        base.PrepareData();
    }

    private void UpdateLoaderConfig()
    {
        string filePath = Path.Combine(SetupData.Paths["12"], "x64\\modules\\config\\ff12-file-loader.ini");
        List<string> lines = File.ReadAllLines(filePath).ToList();

        int pathsStart = lines.FindIndex(s => s.Trim() == "[Paths]");

        lines = lines.Where(s => !s.Trim().StartsWith("rando=")).ToList();
        lines.Insert(pathsStart + 1, "rando=rando");

        File.WriteAllLines(filePath, lines);
    }

    private void CopyLuaScripts()
    {
        string scriptsFolder = Path.Combine(SetupData.Paths["12"], "x64\\scripts");

        FileHelpers.CopyFromFolder(scriptsFolder, "data\\scripts");
    }

    public void RemoveAndMoveLuaScripts()
    {
        string scriptsFolder = Path.Combine(SetupData.Paths["12"], "x64\\scripts");

        Directory.GetFiles(scriptsFolder).Where(s => Path.GetFileName(s).StartsWith("Rando")).ForEach(s => File.Delete(s));

        string descriptiveFolder = $"{SetupData.Paths["12"]}\\x64\\scripts\\config\\TheInsurgentsDescriptiveInventoryConfig";
        if (!File.Exists(Path.Combine(descriptiveFolder, "us.lua.before_rando")))
        {
            MoveToBackup(Path.Combine(descriptiveFolder, "us.lua"), false);
        }

        // Move the original script to a backup if it hasn't been moved already
        if (!File.Exists(Path.Combine(SetupData.Paths["12"], "x64\\scripts\\config\\TheInsurgentsManifestoConfig.lua.before_rando")))
        {
            MoveToBackup(Path.Combine(SetupData.Paths["12"], "x64\\scripts\\config\\TheInsurgentsManifestoConfig.lua"), false);
        }
    }

    protected override void GeneratePack()
    {
        // No pack for FF12
    }

    protected override void GeneratePackAndDocs()
    {
        RandoUI.SetUIProgressIndeterminate("Generating documentation...");

        base.GeneratePackAndDocs();

        RandoUI.SetUIProgressDeterminate($"Complete! Ready to play! The documentation has been generated in the docs folder of this application.", 100, 100);
    }
    public static bool ToolsInstalled()
    {
        return ToolPaths.All(s => File.Exists(s));
    }

    public static bool FileLoaderInstalled()
    {
        if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            return false;
        }

        return FileLoaderPaths.All(s => File.Exists(s));
    }

    public static void UninstallFileLoader()
    {
        FileLoaderPaths.Where(s => File.Exists(s)).ForEach(s => File.Delete(s));
    }

    public static bool LuaLoaderInstalled()
    {
        if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            return false;
        }

        return LuaLoaderPaths.All(s => File.Exists(s));
    }

    public static void UninstallLuaLoader()
    {
        LuaLoaderPaths.Where(s => File.Exists(s)).ForEach(s => File.Delete(s));
    }

    public static bool DescriptiveInstalled()
    {
        if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            return false;
        }

        return DescriptivePaths.All(s => File.Exists(s));
    }

    public static void UninstallDescriptive()
    {
        DescriptivePaths.Where(s => File.Exists(s)).ForEach(s => File.Delete(s));
    }

    public enum ManifestoInstallType
    {
        Missing,
        Vortex,
        Rando
    }

    public static ManifestoInstallType ManifestoInstalled()
    {
        if (!SetupData.Paths.ContainsKey("12") || !Directory.Exists(SetupData.Paths["12"]))
        {
            return ManifestoInstallType.Missing;
        }

        if (ManifestoRequiredPathsRandoInstall.All(s => File.Exists(s)) && Directory.Exists(Path.Combine(SetupData.Paths["12"], "rando\\ps2data\\obj_finish\\in\\chara")))
        {
            return ManifestoInstallType.Rando;
        }

        if (ManifestoRequiredPathsVortexInstall.All(s => File.Exists(s)) && Directory.Exists(Path.Combine(SetupData.Paths["12"], "mods\\deploy\\ps2data\\obj_finish\\in\\chara")))
        {
            return ManifestoInstallType.Vortex;
        }

        return ManifestoInstallType.Missing;
    }

    public static void UninstallManifesto()
    {
        if (ManifestoRequiredPathsVortexInstall.All(s => File.Exists(s)) && Directory.Exists(Path.Combine(SetupData.Paths["12"], "mods\\deploy\\ps2data\\obj_finish\\in\\chara")))
        {
                string filePath = Path.Combine(SetupData.Paths["12"], "x64\\scripts\\config\\TheInsurgentsManifestoConfig.lua");
            if (File.Exists(filePath + ".before_rando"))
            {
                // Ask user whether to restore backed up lua file or delete it
                if (MessageBox.Show("Detected that the Insurgent's Manifesto was installed through Vortex. Would you like to revert the original Insurgent's Manifesto file for the Vortex install?\nYes - Revert\nNo - Delete",
                    "Revert or Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // Restore config
                    File.Delete(filePath);
                    File.Move(filePath + ".before_rando", filePath);

                    return;
                }
            }
            else
            {
                MessageBox.Show("Detected that the Insurgent's Manifesto was installed through Vortex. However, the backup Insurgent's Manifesto config file for the Vortex install was not found. Nothing to revert. Uninstall through Vortex if you want to completely remove the Insurgent's Manifesto mod.", "Nothing to revert", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Delete including config backup        
        List<string> manifestoScripts = new()
        {
            Path.Combine(SetupData.Paths["12"], "x64\\scripts\\TheInsurgentsManifesto.lua"),
            Path.Combine(SetupData.Paths["12"], "x64\\scripts\\config\\TheInsurgentsManifestoConfig.lua"),
            Path.Combine(SetupData.Paths["12"], "x64\\scripts\\config\\TheInsurgentsManifestoConfig.lua.before_rando")
        };
        manifestoScripts.Where(s => File.Exists(s)).ForEach(s => File.Delete(s));
    }

    public static void MoveToBackup(string path, bool makeCopy = false)
    {
        if (File.Exists(path) && !File.Exists(path + ".before_rando"))
        {
            if (makeCopy)
            {
                File.Copy(path, path + ".before_rando");
            }
            else
            {
                File.Move(path, path + ".before_rando");
            }
        }
    }
}
