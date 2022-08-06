using System.Linq;

namespace Bartz24.RandoWPF
{
    public class StatValuesWeighted : StatValues
    {
        private int[] weights;
        public StatValuesWeighted(int[] weights) : base(weights.Length)
        {
            this.weights = weights;
        }

        protected override int SelectNext()
        {
            return RandomNum.SelectRandomWeighted(Enumerable.Range(0, weights.Length).ToList(), i => weights[i]);
        }
    }
}
