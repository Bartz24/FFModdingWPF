using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;

// Uses ZLib level 9 to compress the JSON string for the flags
public class FlagStringCompressor
{   
    public FlagStringCompressor()
    {
    }

    public string CompressFlags()
    {
        return Compress(RandoFlags.Serialize(SetupData.Seed, SetupData.Version));
    }

    public void DecompressToFile(string compressed, string file)
    {
        string decompressed = Decompress(compressed);

        System.IO.File.WriteAllText(file, decompressed);
    }

    public void DecompressLoadFlags(string compressed)
    {
        string decompressed = Decompress(compressed);

        RandoFlags.Deserialize(decompressed);
    }

    public string Compress(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        using (System.IO.MemoryStream outputStream = new())
        {
            using (System.IO.Compression.DeflateStream deflateStream = new(outputStream, System.IO.Compression.CompressionLevel.Optimal))
            {
                deflateStream.Write(inputBytes, 0, inputBytes.Length);
            }

            return Convert.ToBase64String(outputStream.ToArray());
        }
    }

    public string Decompress(string input)
    {
        byte[] inputBytes = Convert.FromBase64String(input);

        using (System.IO.MemoryStream inputStream = new(inputBytes))
        {
            using (System.IO.Compression.DeflateStream deflateStream = new(inputStream, System.IO.Compression.CompressionMode.Decompress))
            {
                using (System.IO.MemoryStream outputStream = new())
                {
                    deflateStream.CopyTo(outputStream);

                    return Encoding.UTF8.GetString(outputStream.ToArray());
                }
            }
        }
    }
}
