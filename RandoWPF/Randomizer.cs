using Bartz24.Docs;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Bartz24.RandoWPF;

public class Randomizer
{
    public RandomizerManager Randomizers { get; }

    public Randomizer(RandomizerManager randomizers)
    {
        Randomizers = randomizers;
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

public interface IPlandomizer
{

    public UserControl GetPlandoPage()
    {
        return null;
    }

    public void SetState(object state)
    {

    }

}

public interface PlandoPage
{
    event Action<object> OnComplete;
}