namespace Bartz24.Data.Tests;

[TestClass()]
public class ValueBinaryTests
{
    [TestMethod()]
    [DataRow((byte)0b10110110, 0, false)]
    [DataRow((byte)0b10110110, 1, true)]
    [DataRow((byte)0b10110110, 2, true)]
    [DataRow((byte)0b10110110, 3, false)]
    [DataRow((byte)0b10110110, 4, true)]
    [DataRow((byte)0b10110110, 5, true)]
    [DataRow((byte)0b10110110, 6, false)]
    [DataRow((byte)0b10110110, 7, true)]
    public void ReadBinary(byte value, int binaryIndex, bool expected)
    {
        bool actual = new byte[] { value }.ReadBinary(0, binaryIndex);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod()]
    [DataRow((byte)0b00000000, 0, true, (byte)0b00000001)]
    [DataRow((byte)0b00000000, 1, true, (byte)0b00000010)]
    [DataRow((byte)0b00000000, 2, true, (byte)0b00000100)]
    [DataRow((byte)0b00000000, 3, true, (byte)0b00001000)]
    [DataRow((byte)0b00000000, 4, true, (byte)0b00010000)]
    [DataRow((byte)0b00000000, 5, true, (byte)0b00100000)]
    [DataRow((byte)0b00000000, 6, true, (byte)0b01000000)]
    [DataRow((byte)0b00000000, 7, true, (byte)0b10000000)]
    [DataRow((byte)0b11111111, 0, false, (byte)0b11111110)]
    [DataRow((byte)0b11111111, 1, false, (byte)0b11111101)]
    [DataRow((byte)0b11111111, 2, false, (byte)0b11111011)]
    [DataRow((byte)0b11111111, 3, false, (byte)0b11110111)]
    [DataRow((byte)0b11111111, 4, false, (byte)0b11101111)]
    [DataRow((byte)0b11111111, 5, false, (byte)0b11011111)]
    [DataRow((byte)0b11111111, 6, false, (byte)0b10111111)]
    [DataRow((byte)0b11111111, 7, false, (byte)0b01111111)]
    public void SetBinary(byte original, int binaryIndex, bool value, byte expected)
    {
        byte[] actual = new byte[] { original };
        actual.SetBinary(0, binaryIndex, value);
        Assert.AreEqual(expected, actual[0]);
    }
}