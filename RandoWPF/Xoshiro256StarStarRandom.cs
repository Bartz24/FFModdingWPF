// Ported from Redzen's Xoshiro256StarStarRandom and RandomSourceBase:
//    https://github.com/colgreen/Redzen/blob/main/Redzen/Random/Xoshiro256StarStarRandom.cs
//
// Redzen code library.
// Copyright 2015-2023 Colin D. Green (colin.green1@gmail.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// xoshiro256** itself: written in 2018 by David Blackman and Sebastiano Vigna (vigna@acm.org) and dedicated to the
// public domain (http://creativecommons.org/publicdomain/zero/1.0/); see http://prng.di.unimi.it/.
using System;
using System.Buffers.Binary;
using System.Numerics;

namespace Bartz24.RandoWPF;

/// <summary>
/// Xoshiro256** (xor, shift, rotate) pseudo-random number generator, usable anywhere a <see cref="Random"/> is.
/// </summary>
/// <remarks>
/// A seeded System.Random's first values are nearly linear in the seed, so flags seeded a fixed distance apart made
/// correlated first picks. The seed is hashed with splitmix64 here, so nearby seeds give unrelated streams.
/// Argument handling (e.g. Next(5, 5) returning 5) matches System.Random.
/// </remarks>
public sealed class Xoshiro256StarStarRandom : Random
{
    private const double INCR_DOUBLE = 1.0 / (1UL << 53);
    private const float INCR_FLOAT = 1f / (1U << 24);

    // RNG state.
    private ulong s0;
    private ulong s1;
    private ulong s2;
    private ulong s3;

    /// <summary>
    /// Initialises a new instance with a random seed.
    /// </summary>
    public Xoshiro256StarStarRandom()
        : this(Shared.NextInt64(long.MinValue, long.MaxValue))
    {
    }

    /// <summary>
    /// Initialises a new instance with the provided seed.
    /// </summary>
    public Xoshiro256StarStarRandom(long seed)
    {
        Reinitialise(unchecked((ulong)seed));
    }

    public void Reinitialise(ulong seed)
    {
        // The first sample is strongly correlated with the state, so the seed's bits are hashed with splitmix64.
        // Splitmix64 is equidistributed, so the state can never be all zeroes (which xoshiro requires).
        s0 = Splitmix64(ref seed);
        s1 = Splitmix64(ref seed);
        s2 = Splitmix64(ref seed);
        s3 = Splitmix64(ref seed);
    }

    private static ulong Splitmix64(ref ulong x)
    {
        ulong z = x += 0x9E3779B97F4A7C15UL;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    /// <summary>
    /// The next 64 random bits.
    /// </summary>
    public ulong NextULong()
    {
        ulong result = BitOperations.RotateLeft(s1 * 5, 7) * 9;

        ulong t = s1 << 17;
        s2 ^= s0;
        s3 ^= s1;
        s1 ^= s2;
        s0 ^= s3;
        s2 ^= t;
        s3 = BitOperations.RotateLeft(s3, 45);

        return result;
    }

    /// <summary>
    /// A random integer in [0, int.MaxValue).
    /// </summary>
    public override int Next()
    {
        ulong rtn;
        do
        {
            rtn = NextULong() >> 33;
        }
        while (rtn == int.MaxValue);

        return (int)rtn;
    }

    /// <summary>
    /// A random integer in [0, maxValue), or 0 when maxValue is 0.
    /// </summary>
    public override int Next(int maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxValue);
        return (int)NextBelow((ulong)maxValue);
    }

    /// <summary>
    /// A random integer in [minValue, maxValue), or minValue when they are equal.
    /// </summary>
    public override int Next(int minValue, int maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue);
        return (int)((long)NextBelow((ulong)((long)maxValue - minValue)) + minValue);
    }

    /// <summary>
    /// A random integer in [0, long.MaxValue).
    /// </summary>
    public override long NextInt64()
    {
        ulong rtn;
        do
        {
            rtn = NextULong() >> 1;
        }
        while (rtn == long.MaxValue);

        return (long)rtn;
    }

    /// <summary>
    /// A random integer in [0, maxValue), or 0 when maxValue is 0.
    /// </summary>
    public override long NextInt64(long maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxValue);
        return (long)NextBelow((ulong)maxValue);
    }

    /// <summary>
    /// A random integer in [minValue, maxValue), or minValue when they are equal.
    /// </summary>
    public override long NextInt64(long minValue, long maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minValue, maxValue);
        return unchecked((long)NextBelow((ulong)(maxValue - minValue)) + minValue);
    }

    /// <summary>
    /// A random double in [0, 1).
    /// </summary>
    public override double NextDouble()
    {
        return (NextULong() >> 11) * INCR_DOUBLE;
    }

    /// <summary>
    /// A random float in [0, 1).
    /// </summary>
    public override float NextSingle()
    {
        return (NextULong() >> 40) * INCR_FLOAT;
    }

    protected override double Sample()
    {
        return NextDouble();
    }

    public override void NextBytes(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        NextBytes(buffer.AsSpan());
    }

    public override void NextBytes(Span<byte> buffer)
    {
        while (buffer.Length >= sizeof(ulong))
        {
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, NextULong());
            buffer = buffer.Slice(sizeof(ulong));
        }

        if (!buffer.IsEmpty)
        {
            Span<byte> last = stackalloc byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64LittleEndian(last, NextULong());
            last.Slice(0, buffer.Length).CopyTo(buffer);
        }
    }

    /// <summary>
    /// An unbiased random integer in [0, count), or 0 when count is 0 or 1.
    /// </summary>
    private ulong NextBelow(ulong count)
    {
        if (count <= 1)
        {
            return 0;
        }

        // Rejection sampling: take the fewest high bits that can represent count values and retry when the value is
        // out of range. The high bits are used as they are the most random.
        int bitCount = 64 - BitOperations.LeadingZeroCount(count - 1);
        ulong x;
        do
        {
            x = NextULong() >> (64 - bitCount);
        }
        while (x >= count);

        return x;
    }
}
