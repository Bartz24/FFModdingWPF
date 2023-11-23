using Bartz24.Docs;
using System.Collections.Generic;

namespace Bartz24.RandoWPF;

public class Randomizer
{
    public SeedGenerator Generator { get; }

    public Randomizer(SeedGenerator generator)
    {
        Generator = generator;
    }

    public virtual void Load()
    {

    }

    public virtual void Randomize()
    {

    }

    public virtual void Save()
    {

    }

    public virtual Dictionary<string, HTMLPage> GetDocumentation()
    {
        return new Dictionary<string, HTMLPage>();
    }
}
