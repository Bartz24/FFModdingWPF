using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public class AllowMatrix
{
    private bool?[,] matrix;

    private Dictionary<string, int> namesToIndex = new ();

    public AllowMatrix(int size, List<string> names)
    {
        matrix = new bool?[size, size];
        for (int i = 0; i < names.Count; i++)
        {
            namesToIndex.Add(names[i], i);
        }
    }

    public void AddAllow(string location, string replacement, bool value)
    {
        matrix[namesToIndex[location], namesToIndex[replacement]] = value;
    }

    public bool HasAllow(string location, string replacement)
    {
        return matrix[namesToIndex[location], namesToIndex[replacement]].HasValue;
    }

    public bool IsAllowed(string location, string replacement)
    {
        if (!matrix[namesToIndex[location], namesToIndex[replacement]].HasValue)
        {
            throw new Exception($"AllowMatrix does not contain {location} to {replacement}.");
        }

        return matrix[namesToIndex[location], namesToIndex[replacement]].Value;
    }
}

