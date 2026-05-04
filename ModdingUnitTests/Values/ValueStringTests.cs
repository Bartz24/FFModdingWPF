using System.Text;

namespace Bartz24.Data.Tests;

[TestClass()]
public class ValueStringTests
{
    [TestMethod()]
    [DataRow("")]
    [DataRow("Hello")]
    [DataRow("World")]
    [DataRow("Test String")]
    [DataRow("Special chars: àáâãäåæçèéêë")]
    public void ReadString(string value)
    {
        byte[] data = Encoding.UTF8.GetBytes(value).Concat(new byte[] { 0 }).ToArray();
        string actual = data.ReadString(0);
        Assert.AreEqual(value, actual);
    }

    [TestMethod()]
    [DataRow("")]
    [DataRow("Hello")]
    [DataRow("World")]
    [DataRow("Test String")]
    [DataRow("Special chars: àáâãäåæçèéêë")]
    public void SetString(string value)
    {
        byte[] actual = new byte[Encoding.UTF8.GetByteCount(value) + 10]; // Extra space for UTF-8 and null terminator
        actual.SetString(0, value);
        string result = actual.ReadString(0);
        Assert.AreEqual(value, result);
    }

    [TestMethod()]
    [DataRow("Hello", 10)]
    [DataRow("Test", 8)]
    [DataRow("", 5)]
    public void SetStringWithLength(string value, int length)
    {
        byte[] actual = new byte[length];
        actual.SetString(0, value, length);
        string result = actual.ReadString(0);
        Assert.AreEqual(value, result);
    }
}