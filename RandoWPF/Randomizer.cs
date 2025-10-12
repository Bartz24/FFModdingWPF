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

    /// <summary>
    /// Load any necessary data from data sources
    /// </summary>
    public virtual void Load()
    {

    }

    /// <summary>
    /// Perform any necessary actions after all randomizers have loaded their data
    /// </summary>
    public virtual void PostLoad()
    {

    }

    /// <summary>
    /// Perform the randomization and data modifications based on settings
    /// </summary>
    public virtual void Randomize()
    {

    }

    /// <summary>
    /// Save any modified data back to data files
    /// </summary>
    public virtual void Save()
    {

    }

    /// <summary>
    /// Get documentation pages for this randomizer
    /// </summary>
    /// <returns>
    /// A dictionary mapping page titles to HTMLPage objects
    /// </returns>
    public virtual Dictionary<string, HTMLPage> GetDocumentation()
    {
        return new Dictionary<string, HTMLPage>();
    }
}
