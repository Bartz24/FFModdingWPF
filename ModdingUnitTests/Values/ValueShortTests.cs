namespace Bartz24.Data.Tests;

[TestClass()]
public class ValueShortTests
{
    [TestMethod()]
    [DataRow((short)0)]
    [DataRow((short)1)]
    [DataRow((short)-1)]
    [DataRow((short)32767)]
    [DataRow((short)-32768)]
    [DataRow((short)12345)]
    [DataRow((short)-12345)]
    public void ReadShort(short value)
    {
        byte[] data = new byte[2];
        data.SetShort(0, value);
        short actual = data.ReadShort(0);
        Assert.AreEqual(value, actual);
    }

    [TestMethod()]
    [DataRow((short)0)]
    [DataRow((short)1)]
    [DataRow((short)-1)]
    [DataRow((short)32767)]
    [DataRow((short)-32768)]
    [DataRow((short)12345)]
    [DataRow((short)-12345)]
    public void SetShort(short value)
    {
        byte[] actual = new byte[2];
        actual.SetShort(0, value);
        short result = actual.ReadShort(0);
        Assert.AreEqual(value, result);
    }
}