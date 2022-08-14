using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LRRando
{
    public class MusicRando : Randomizer
    {
        List<string> soundFiles = new List<string>();
        List<string> newSoundFiles = new List<string>();
        Dictionary<string, string> names = new Dictionary<string, string>();
        Dictionary<string, MusicData> musicData = new Dictionary<string, MusicData>();

        public MusicRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            Randomizers.SetProgressFunc("Loading Music Data...", -1, 100);
            musicData.Clear();
            FileHelpers.ReadCSVFile(@"data\musicLR.csv", row =>
            {
                MusicData m = new MusicData(row);
                musicData.Add(m.Path, m);
            }, FileHelpers.CSVFileHeader.HasHeader);
        }
        public override void Randomize(Action<int> progressSetter)
        {
            Randomizers.SetProgressFunc("Randomizing Music Data...", -1, 100);
            if (LRFlags.Other.Music.FlagEnabled)
            {
                LRFlags.Other.Music.SetRand();
                soundFiles = musicData.Keys.ToList();
                if (!LRFlags.Other.FanfareMusic.Enabled)
                    soundFiles = soundFiles.Where(p => !musicData[p].Traits.Contains("DLC")).ToList();
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
            Randomizers.SetProgressFunc("Saving Music Data...", -1, 100);
            for (int i = 0; i < Math.Min(soundFiles.Count, newSoundFiles.Count); i++)
            {
                Directory.CreateDirectory(Path.GetDirectoryName($"{SetupData.OutputFolder}\\{newSoundFiles[i]}"));
                File.Copy($"{Nova.GetNovaFile("LR", soundFiles[i], SetupData.Paths["Nova"], SetupData.Paths["LR"])}", $"{SetupData.OutputFolder}\\{newSoundFiles[i]}", true);
            }
        }

        public class MusicData
        {
            public string Path { get; set; }
            public List<string> Traits { get; set; }
            public MusicData(string[] row)
            {
                Path = row[0];
                Traits = row[1].Split("|").ToList();
            }
        }
    }
}
