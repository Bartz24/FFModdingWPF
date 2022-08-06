using Bartz24.Docs;
using System;

namespace Bartz24.RandoWPF
{
    public class Randomizer
    {
        public RandomizerManager Randomizers { get; }

        public Randomizer(RandomizerManager randomizers)
        {
            this.Randomizers = randomizers;
        }

        public virtual void Load()
        {

        }

        public virtual void Randomize(Action<int> progressSetter)
        {

        }

        public virtual void Save()
        {

        }

        public virtual HTMLPage GetDocumentation()
        {
            return null;
        }

        public virtual string GetID()
        {
            return "NONE";
        }
    }
}
