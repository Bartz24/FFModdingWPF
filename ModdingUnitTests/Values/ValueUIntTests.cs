namespace Bartz24.Data.Tests;

[TestClass()]
public class ValueUIntTests
{
    [TestMethod()]
    [DataRow((uint)0)]
    [DataRow((uint)1)]
    [DataRow((uint)4294967295)]
    [DataRow((uint)2147483647)]
    [DataRow((uint)2147483648)]
    [DataRow((uint)12345678)]
    [DataRow((uint)987654321)]
    public void ReadUInt(uint value)
    {
        byte[] data = new byte[4];
        data.SetUInt(0, value);
        uint actual = data.ReadUInt(0);
        Assert.AreEqual(value, actual);
    }

    [TestMethod()]
    [DataRow((uint)0)]
    [DataRow((uint)1)]
    [DataRow((uint)4294967295)]
    [DataRow((uint)2147483647)]
    [DataRow((uint)2147483648)]
    [DataRow((uint)12345678)]
    [DataRow((uint)987654321)]
    public void SetUInt(uint value)
    {
        byte[] actual = new byte[4];
        actual.SetUInt(0, value);
        uint result = actual.ReadUInt(0);
        Assert.AreEqual(value, result);
    }
}