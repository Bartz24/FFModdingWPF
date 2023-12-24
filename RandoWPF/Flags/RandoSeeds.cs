using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Bartz24.RandoWPF;
public class RandoSeeds
{
    public static List<SeedInformation> Seeds { get; set; } = new ();

    public static string DocsFolder { get; set; } = "packs";

    public static string DeleteFilter { get; set; }

    // Event for when the seeds are loaded
    public static event EventHandler SeedsLoaded;
    public static event EventHandler<DeleteEventArgs> SeedDeleted;

    public static void LoadSeeds()
    {
        Seeds.Clear();

        // Read .zip files from the folder
        string[] packs = Directory.GetFiles(DocsFolder, "*.zip");
        foreach (string pack in packs)
        {
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(pack))
                {
                    // Read the seed information from the .json file of unknown name
                    ZipArchiveEntry entry = archive.Entries.First(x => x.Name.EndsWith(".json"));
                    using (StreamReader reader = new(entry.Open()))
                    {
                        string json = reader.ReadToEnd();
                        (string seed, string version, string preset) = RandoFlags.GetSeedInfo(json);
                        FlagStringCompressor compressor = new();
                        Seeds.Add(new SeedInformation()
                        {
                            Seed = seed,
                            Created = entry.LastWriteTime.DateTime,
                            Version = version,
                            FlagString = compressor.Compress(json),
                            PresetUsed = preset
                        });
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }

        // Sort by date
        Seeds = Seeds.OrderByDescending(x => x.Created).ToList();

        // Call event
        SeedsLoaded?.Invoke(null, EventArgs.Empty);
    }

    public static void LoadSeed(SeedInformation info)
    {
        FlagStringCompressor compressor = new();
        compressor.DecompressLoadFlags(info.FlagString);
        SetupData.Seed = info.Seed;
        RandoUI.SetUIMessage($"Set the seed to {info.Seed} and loaded flags used for the seed!");

        RandoUI.SwitchUITab(1);
    }

    public static void DeleteSeed(SeedInformation info)
    {
        string updatedFilter = DeleteFilter.Replace("${SEED}", info.Seed);
        // Remove the .zip file matching the filter split by |        
        foreach (string filter in updatedFilter.Split('|'))
        {
            string[] packs = Directory.GetFiles(DocsFolder, filter);
            foreach (string pack in packs)
            {
                try
                {
                    File.Delete(pack);
                }
                catch
                {
                    // Ignore errors
                }
            }
        }

        // Call event
        SeedDeleted?.Invoke(null, new DeleteEventArgs(info));

        RandoUI.SetUIMessage($"Deleted data for the seed {info.Seed}.");
    }

    public static void ShareStringSeed(SeedInformation info)
    {
        // Copy compressed string to clipboard
        Clipboard.SetText(info.FlagString);
        RandoUI.SetUIMessage($"Copied seed string for seed {info.Seed} to clipboard!");
    }

    public static void ShareFileSeed(SeedInformation info)
    {
        FlagStringCompressor compressor = new();
        int seed = RandomNum.GetIntSeed(SetupData.Seed);

        VistaSaveFileDialog dialog = new()
        {
            Filter = "JSON|*.json",
            DefaultExt = ".json",
            AddExtension = true,
            FileName = "FF12Rando_" + seed + "_Seed"
        };
        if ((bool)dialog.ShowDialog())
        {
            string path = dialog.FileName.Replace("/", "\\");
            
            compressor.DecompressToFile(info.FlagString, path);
        }
    }

    public class DeleteEventArgs : EventArgs
    {
        public SeedInformation Information { get; set; }

        public DeleteEventArgs(SeedInformation info)
        {
            Information = info;
        }
    }
}
