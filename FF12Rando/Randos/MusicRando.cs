using Bartz24.Data;
using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF12Rando
{
    public class MusicRando : Randomizer
    {
        List<string> soundFiles = new List<string>();
        List<string> musicPackFiles = new List<string>();
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
            soundFiles.AddRange(File.ReadAllLines("data\\music12.csv"));

            musicPackFiles.AddRange(Directory.GetFiles("data\\musicPacks", "*.mab", SearchOption.AllDirectories));
        }
        public override void Randomize(Action<int> progressSetter)
        {
            if (FF12Flags.Other.Music.FlagEnabled)
            {
                FF12Flags.Other.Music.SetRand();
                soundFiles = soundFiles.Shuffle().ToList();
                newSoundFiles = musicPackFiles.Shuffle().Take(Math.Min(soundFiles.Count, musicPackFiles.Count)).ToList();
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
            Directory.CreateDirectory("outdata\\ps2data\\sound\\music\\magi_data\\win");
            for (int i = 0; i < Math.Min(soundFiles.Count, newSoundFiles.Count); i++)
            {
                File.Copy(newSoundFiles[i], $"outdata\\ps2data\\sound\\music\\magi_data\\win\\{soundFiles[i]}", true);
            }
        }
    }
}
