namespace Bartz24.Data.Tests;

[TestClass()]
public class ValueUShortTests
{
    [TestMethod()]
    [DataRow((ushort)0)]
    [DataRow((ushort)1)]
    [DataRow((ushort)65535)]
    [DataRow((ushort)32767)]
    [DataRow((ushort)32768)]
    [DataRow((ushort)12345)]
    [DataRow((ushort)54321)]
    public void ReadUShort(ushort value)
    {
        byte[] data = new byte[2];
        data.SetUShort(0, value);
        ushort actual = data.ReadUShort(0);
        Assert.AreEqual(value, actual);
    }

    [TestMethod()]
    [DataRow((ushort)0)]
    [DataRow((ushort)1)]
    [DataRow((ushort)65535)]
    [DataRow((ushort)32767)]
    [DataRow((ushort)32768)]
    [DataRow((ushort)12345)]
    [DataRow((ushort)54321)]
    public void SetUShort(ushort value)
    {
        byte[] actual = new byte[2];
        actual.SetUShort(0, value);
        ushort result = actual.ReadUShort(0);
        Assert.AreEqual(value, result);
    }
}