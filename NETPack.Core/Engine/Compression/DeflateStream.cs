using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace NETPack.Core.Engine.Compression
{
    public static class DeflateCompression
    {
        public static byte[] Compress(byte[] raw)
        {
            using (var ms = new MemoryStream(raw))
            {
                using (var ds = new DeflateStream(ms, CompressionMode.Compress))
                {
                    ds.Write(raw, 0, raw.Length);
                    ds.Flush();
                }
                return ms.ToArray();
            }
        }

        public static byte[] Decompress(byte[] compressed)
        {
            var decompressedStream = new MemoryStream();

            using (var ms = new MemoryStream(compressed))
            {
                using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    ds.CopyTo(decompressedStream);
                }
                return decompressedStream.ToArray();
            }
        }
    }
}
