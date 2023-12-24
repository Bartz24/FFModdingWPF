using Bartz24.RandoWPF;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bartz24.RandoWPF.Tests;

[TestClass]
public class FlagStringCompressorTests
{
    private FlagStringCompressor compressor = new();

    [TestMethod]
    public void CompressDecompressTest()
    {
        // Arrange
        var testData = new Dictionary<string, bool>
        {
            {"flag1", true},
            {"flag2", false},
            {"flag3", true},
            {"flag4", false},
            {"flag5", true}
        };

        var jsonString = JsonConvert.SerializeObject(testData);

        // Act
        var compressedString = compressor.Compress(jsonString);
        var decompressedString = compressor.Decompress(compressedString);

        // Assert
        Assert.AreEqual(jsonString, decompressedString);
    }
}
