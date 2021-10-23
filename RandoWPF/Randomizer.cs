using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bartz24.Docs;

namespace Bartz24.RandoWPF
{
    public class Randomizer
    {
        protected RandomizerManager randomizers;

        public Randomizer(RandomizerManager randomizers)
        {
            this.randomizers = randomizers;
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
        /*
        public virtual HTMLPage GetDocumentation()
        {
            return null;
        }*/

        public virtual string GetProgressMessage()
        {
            return "Progressing...";
        }
        public virtual string GetID()
        {
            return "NONE";
        }
    }
}
