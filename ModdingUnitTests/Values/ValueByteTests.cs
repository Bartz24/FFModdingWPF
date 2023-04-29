using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.Data.Tests;

[TestClass()]
public class ValueByteTests
{
    [TestMethod()]
    [DataRow((byte)1)]
    [DataRow((byte)47)]
    [DataRow((byte)226)]
    public void ReadByte(byte value)
    {
        byte actual = new byte[] { value }.ReadByte(0);
        Assert.AreEqual(value, actual);
    }

    [TestMethod()]
    [DataRow((byte)0b10110110, 3, 4, (byte)0b1011)]
    [DataRow((byte)0b10010010, 2, 6, (byte)0b10010)]
    [DataRow((byte)0b10100111, 0, 8, (byte)0b10100111)]
    public void ReadBits(byte value, int bitIndex, int bitLength, byte expected)
    {
        byte actual = new byte[] { value }.ReadByte(0, bitIndex, bitLength);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod()]
    [DataRow((byte)0b10110110, (byte)0b00100111, 3, 8, (byte)0b10110001)]
    [DataRow((byte)0b00100111, (byte)0b10110110, 7, 2, (byte)0b11)]
    [DataRow((byte)0b10100111, (byte)0b00010010, 5, 7, (byte)0b1110001)]
    public void ReadBitsAcrossBytes(byte value, byte value2, int bitIndex, int bitLength, byte expected)
    {
        byte actual = new byte[] { value, value2 }.ReadByte(0, bitIndex, bitLength);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod()]
    [DataRow((byte)1)]
    [DataRow((byte)47)]
    [DataRow((byte)226)]
    public void SetByte(byte value)
    {
        byte[] actual = new byte[] { 0 };
        actual.SetByte(0, value);
        Assert.AreEqual(value, actual[0]);
    }

    [TestMethod()]
    [DataRow((byte)0b1011, 3, 4, (byte)0b10101010, (byte)0b10110110)]
    [DataRow((byte)0b10010, 2, 6, (byte)0b10001110, (byte)0b10010010)]
    [DataRow((byte)0b10100111, 0, 8, (byte)0b11110000, (byte)0b10100111)]
    public void SetBits(byte value, int bitIndex, int bitLength, byte original, byte expected)
    {
        byte[] actual = new byte[] { original };
        actual.SetByte(0, value, bitIndex, bitLength);
        Assert.AreEqual(expected, actual[0]);
    }

    [TestMethod()]
    [DataRow((byte)0b10110001, 3, 8, (byte)0b10100001, (byte)0b11100111, (byte)0b10110110, (byte)0b00100111)]
    [DataRow((byte)0b11, 7, 2, (byte)0b00100110, (byte)0b00110110, (byte)0b00100111, (byte)0b10110110)]
    [DataRow((byte)0b1110001, 5, 7, (byte)0b10100101, (byte)0b11100010, (byte)0b10100111, (byte)0b00010010)]
    public void SetBitsAcrossBytes(byte value, int bitIndex, int bitLength, byte original, byte original2, byte expected, byte expected2)
    {
        byte[] actual = new byte[] { original, original2 };
        actual.SetByte(0, value, bitIndex, bitLength);
        Assert.AreEqual(expected, actual[0]);
        Assert.AreEqual(expected2, actual[1]);
    }
}