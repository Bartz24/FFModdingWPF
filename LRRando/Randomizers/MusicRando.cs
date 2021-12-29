using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRRando
{
    public class MusicRando : Randomizer
    {
        List<string> soundFiles = new List<string>();
        List<string> newSoundFiles = new List<string>();
        Dictionary<string, string> names = new Dictionary<string, string>();

        public MusicRando(RandomizerManager randomizers) : base(randomizers) {  }

        public override string GetProgressMessage()
        {
            return "Randomizing Music...";
        }
        public override string GetID()
        {
            return "Music";
        }

        public override void Load()
        {
            soundFiles.AddRange(File.ReadAllLines("data\\musicLR.csv"));
        }
        public override void Randomize(Action<int> progressSetter)
        {
            if (LRFlags.Other.Music.FlagEnabled)
            {
                LRFlags.Other.Music.SetRand();
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
            for (int i = 0; i < Math.Min(soundFiles.Count, newSoundFiles.Count); i++)
            {
                Directory.CreateDirectory(Path.GetDirectoryName($"{SetupData.OutputFolder}\\{newSoundFiles[i]}"));
                File.Copy($"{Nova.GetNovaFile("LR", soundFiles[i], SetupData.Paths["Nova"], SetupData.Paths["LR"])}", $"{SetupData.OutputFolder}\\{newSoundFiles[i]}", true);
            }
        }
    }
}
