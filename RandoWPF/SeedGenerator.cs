using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Threading.Tasks;
using Bartz24.Data;
using Bartz24.Docs;
using System.Reflection.Emit;
using Microsoft.Extensions.Logging;

namespace Bartz24.RandoWPF;

public class SeedGenerator : IDisposable
{
    public const string UNEXPECTED_ERROR = "Unexpected error";

    public Action<string, int, int> SetUIProgress { get; set; }
    public Action IncrementTotalProgress { get; set; }
    public string DataOutFolder { get; set; }

    public string OutFolder { get; set; }
    public string DocsOutFolder { get; set; } = "packs";
    public string PackPrefixName { get; set; }
    public string DocsDisplayName { get; set; }

    public List<Randomizer> Randomizers { get; set; }

    public FileLogger Logger { get; set; }

    public SeedGenerator()
    {
        Randomizers = new();
    }

    public T Get<T>() where T : Randomizer
    {
        foreach (Randomizer randomizer in Randomizers)
        {
            if (randomizer.GetType() == typeof(T))
            {
                return (T)randomizer;
            }
        }

        return null;
    }

    public void GenerateSeed()
    {
        Logger = new FileLogger("RandoLog.log", LogLevel.Debug);

        // Log the version and the seed
        Logger.LogInformation($"Version: {SetupData.Version}");
        Logger.LogInformation($"Seed: {SetupData.Seed}");
        FlagStringCompressor compressor = new();
        Logger.LogInformation($"Flags: {compressor.CompressFlags()}");

        try
        {
            SetUIProgress("Preparing data folder...", -1, -1);
            PrepareData();
            IncrementTotalProgress();

            SetUIProgress("Loading data...", -1, -1);
            Load();

            SetUIProgress("Randomizing data...", -1, -1);
            Randomize();

            SetUIProgress("Saving data...", -1, -1);
            Save();

            SetUIProgress("Generating modpack and documentation...", -1, -1);
            GeneratePackAndDocs();
            IncrementTotalProgress();
        }
        catch (RandoException ex)
        {
            throw ex;
        }
        catch (Exception ex)
        {
            throw new RandoException(ex.Message, UNEXPECTED_ERROR, ex);
        }
    }

    protected virtual void PrepareData()
    {
        RandomNum.ClearRand();
        foreach (Flag flag in RandoFlags.FlagsList)
        {
            flag.ResetRandom(GetIntSeed());
        }
    }

    public int GetIntSeed()
    {
        return RandomNum.GetIntSeed(SetupData.Seed);
    }

    protected virtual void Load()
    {
        Randomizers.ForEach(r =>
        {
            r.Load();
            IncrementTotalProgress();
        });
    }

    protected virtual void Randomize()
    {
        Randomizers.ForEach(r =>
        {
            r.Randomize();
            IncrementTotalProgress();
        });
    }

    protected virtual void Save()
    {
        Randomizers.ForEach(r =>
        {
            r.Save();
            IncrementTotalProgress();
        });
    }

    protected virtual void GeneratePackAndDocs()
    {
        Task pack = new(GeneratePack);
        Task docs = new(GenerateDocs);

        pack.Start();
        docs.Start();
        Task.WaitAll(pack, docs);
    }


    protected virtual void GeneratePack()
    {
        string zipName = "packs\\" + GetPackPath();
        if (File.Exists(zipName))
        {
            File.Delete(zipName);
        }

        if (!Directory.Exists("packs"))
        {
            Directory.CreateDirectory("packs");
        }

        ZipFile.CreateFromDirectory(OutFolder, zipName);

        Directory.Delete(OutFolder, true);
    }

    public virtual string GetPackPath()
    {
        return $"{PackPrefixName}_{GetIntSeed()}.zip";
    }

    protected virtual void GenerateDocs()
    {
        Docs.Docs docs = new();
        docs.Settings.Name = DocsDisplayName;
        Randomizers.ForEach(r =>
        {
            Dictionary<string, HTMLPage> pages = r.GetDocumentation();
            pages.ForEach(p => docs.AddPage(p.Key, p.Value));
        });

        docs.Generate($"{DocsOutFolder}\\docs_latest", @"data\docs\template");
        RandoHelpers.SaveSeedJSON($"{DocsOutFolder}\\docs_latest\\{PackPrefixName}_{GetIntSeed()}_Seed.json");
        string zipDocsName = $"{DocsOutFolder}\\{PackPrefixName}_{GetIntSeed()}_Docs.zip";
        if (File.Exists(zipDocsName))
        {
            File.Delete(zipDocsName);
        }

        ZipFile.CreateFromDirectory($"{DocsOutFolder}\\docs_latest", zipDocsName);
    }

    public void Dispose()
    {
        Logger.Dispose();
    }
}