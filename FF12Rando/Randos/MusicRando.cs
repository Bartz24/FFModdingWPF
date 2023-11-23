using Bartz24.RandoWPF;
using FF12Rando;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FF12Rando;

public class MusicRando : Randomizer
{
    private List<string> soundFiles = new();
    private readonly List<string> musicPackFiles = new();
    private List<string> newSoundFiles = new();
    private readonly Dictionary<string, string> names = new();

    public MusicRando(SeedGenerator randomizers) : base(randomizers) { }

    public override void Load()
    {
        Generator.SetUIProgress("Loading Music Data...", 0, -1);
        soundFiles.AddRange(File.ReadAllLines("data\\music12.csv"));

        musicPackFiles.AddRange(Directory.GetFiles("data\\musicPacks", "*.mab", SearchOption.AllDirectories));
    }
    public override void Randomize()
    {
        Generator.SetUIProgress("Randomizing Music Data...", 0, -1);
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
        Generator.SetUIProgress("Saving Music Data...", 0, -1);
        Directory.CreateDirectory($"{Generator.DataOutFolder}\\sound\\music\\magi_data\\win");
        for (int i = 0; i < Math.Min(soundFiles.Count, newSoundFiles.Count); i++)
        {
            File.Copy(newSoundFiles[i], $"{Generator.DataOutFolder}\\sound\\music\\magi_data\\win\\{soundFiles[i]}", true);
        }
    }
}
