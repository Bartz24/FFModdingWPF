using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF
{
    public class RandomizerManager : List<Randomizer>
    {
        public Action<string, int, int> SetProgressFunc { get; set; }
        public T Get<T>(string id) where T : Randomizer
        {
            foreach(Randomizer randomizer in this)
            {
                if (randomizer.GetID() == id)
                    return (T)randomizer;
            }
            return null;
        }
    }
}
