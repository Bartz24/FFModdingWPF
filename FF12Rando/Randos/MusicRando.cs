using Bartz24.RandoWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando
{
    public class MusicRando : Randomizer
    {
        List<string> soundFiles = new List<string>();
        List<string> musicPackFiles = new List<string>();
        List<string> newSoundFiles = new List<string>();
        Dictionary<string, string> names = new Dictionary<string, string>();

        public MusicRando(RandomizerManager randomizers) : base(randomizers) { }

        public override void Load()
        {
            Randomizers.SetUIProgress("Loading Music Data...", 0, -1);
            soundFiles.AddRange(File.ReadAllLines("data\\music12.csv"));

            musicPackFiles.AddRange(Directory.GetFiles("data\\musicPacks", "*.mab", SearchOption.AllDirectories));
        }
        public override void Randomize()
        {
            Randomizers.SetUIProgress("Randomizing Music Data...", 0, -1);
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
            Randomizers.SetUIProgress("Saving Music Data...", 0, -1);
            Directory.CreateDirectory("outdata\\ps2data\\sound\\music\\magi_data\\win");
            for (int i = 0; i < Math.Min(soundFiles.Count, newSoundFiles.Count); i++)
            {
                File.Copy(newSoundFiles[i], $"outdata\\ps2data\\sound\\music\\magi_data\\win\\{soundFiles[i]}", true);
            }
        }
    }
}
