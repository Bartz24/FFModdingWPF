using System.Collections.Generic;

namespace Bartz24.Data;

public class StringHasher
{
    // A set to keep track of used hash values.
    private HashSet<uint> UsedValues { get; set; }

    private uint NumBits { get; set; }

    public StringHasher(uint numBits = 32)
    {
        UsedValues = new();
        NumBits = numBits;
    }

    // Public method to get a hash code for a string.
    public uint GetUniqueHash(string str)
    {
        // Calculate the initial hash value.
        uint hash = ComputeHash(str);

        // Find a unique hash by incrementing if there are conflicts.
        while (UsedValues.Contains(hash))
        {
            hash = IncrementAndWrap(hash);
        }

        // Add the unique hash to the used values set.
        UsedValues.Add(hash);

        return hash;
    }

    // Method to compute the initial hash value for a string.
    private uint ComputeHash(string str)
    {
        unchecked
        {
            ulong hash = 0;
            foreach (char c in str)
            {
                hash = (hash * NumBits) + c;
            }

            return (uint)(hash & (2 ^ NumBits - 1));
        }
    }

    // Method to increment the hash value and wrap around within the bit range.
    private uint IncrementAndWrap(uint hash)
    {
        return (uint)((hash + 1) & (2 ^ NumBits - 1));
    }
}