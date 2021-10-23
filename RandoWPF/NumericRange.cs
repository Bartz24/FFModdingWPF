using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF
{
    public class NumericRange<T> where T : IComparable
    {
        public EventHandler OnValueChanged, OnAnyValueChanged;
        private T val, min, max;
        private int decimalPlaces;
        public T MinRange
        {
            get => min;
            set
            {
                if (value.CompareTo(MaxRange) > 0)
                    MaxRange = value;
                if (value.CompareTo(Value) > 0)
                    Value = value;
                min = value;
                OnAnyValueChanged?.Invoke(this, null);
            }
        }
        public T Value {
            get => val;
            set
            {
                if (value.CompareTo(MaxRange) > 0)
                    val = MaxRange;
                else if (value.CompareTo(MinRange) < 0)
                    val = MinRange;
                else
                    val = value;
                OnValueChanged?.Invoke(this, null);
                OnAnyValueChanged?.Invoke(this, null);
            }
        }
        public T MaxRange
        {
            get => max;
            set
            {
                if (value.CompareTo(MinRange) < 0)
                    MinRange = value;
                if (value.CompareTo(Value) < 0)
                    Value = value;
                max = value;
                OnAnyValueChanged?.Invoke(this, null);
            }
        }
        public int DecimalPlaces
        {
            get => decimalPlaces;
            set
            {
                decimalPlaces = value;
            }
        }
    }
}
