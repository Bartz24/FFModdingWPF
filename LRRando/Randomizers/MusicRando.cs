using Bartz24.Data;
using Bartz24.RandoWPF;
using LRRando;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LRRando;

public class MusicRando : Randomizer
{
    private List<string> soundFiles = new();
    private List<string> newSoundFiles = new();
    private readonly Dictionary<string, string> names = new();
    private readonly Dictionary<string, MusicData> musicData = new();

    public MusicRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        Randomizers.SetUIProgress("Loading Music Data...", -1, 100);
        musicData.Clear();
        FileHelpers.ReadCSVFile(@"data\musicLR.csv", row =>
        {
            MusicData m = new(row);
            musicData.Add(m.Path, m);
        }, FileHelpers.CSVFileHeader.HasHeader);
    }
    public override void Randomize()
    {
        Randomizers.SetUIProgress("Randomizing Music Data...", -1, 100);
        if (LRFlags.Other.Music.FlagEnabled)
        {
            LRFlags.Other.Music.SetRand();
            soundFiles = musicData.Keys.ToList();
            if (!LRFlags.Other.FanfareMusic.Enabled)
            {
                soundFiles = soundFiles.Where(p => !musicData[p].Traits.Contains("DLC")).ToList();
            }

            newSoundFiles = soundFiles.Shuffle().ToList();
            RandomNum.ClearRand();
        }
    }
    /*
    public override HTMLPage GetDocumentation()
    {
        HTMLPage page = new HTMLPage("Music", "template/documentation.html");
        page.HTMLElements.Add(new Table("Music", new string[] { "Original Track", "New Track" }.ToList(), new int[] { 50, 50 }.ToList(), Enumerable.Range(0, soundFiles.Count).Select(i => new string[] { names[soundFiles[i]], names[newSoundFiles[i]] }.ToList()).ToList())); ;
        return page;
    }*/

    public override void Save()
    {
        Randomizers.SetUIProgress("Saving Music Data...", -1, 100);
        for (int i = 0; i < Math.Min(soundFiles.Count, newSoundFiles.Count); i++)
        {
            Directory.CreateDirectory(Path.GetDirectoryName($"{SetupData.OutputFolder}\\{newSoundFiles[i]}"));
            File.Copy($"{Nova.GetNovaFile("LR", soundFiles[i], SetupData.Paths["Nova"], SetupData.Paths["LR"])}", $"{SetupData.OutputFolder}\\{newSoundFiles[i]}", true);
        }
    }

    public class MusicData : CSVDataRow
    {
        [RowIndex(0)]
        public string Path { get; set; }
        [RowIndex(1)]
        public List<string> Traits { get; set; }
        public MusicData(string[] row) : base(row)
        {
        }
    }
}
