using System.Linq;

namespace Bartz24.RandoWPF
{
    public class StatValuesChanceSelect : StatValues
    {
        private int[] chances;
        public StatValuesChanceSelect(int[] chances) : base(chances.Length)
        {
            this.chances = chances;
        }

        protected override int SelectNext()
        {
            return RandomNum.SelectRandomWeighted(Enumerable.Range(0, chances.Length).ToList(), i => chances[i]);
        }
    }
}
