using System.Collections.Generic;
using System.IO;

namespace Kavod.Vba.Compression
{
    /// <summary>
    /// The DecompressedBuffer is a resizable array of bytes that contains the same data as the 
    /// CompressedContainer (section 2.4.1.1.1), but the data is in an uncompressed format.
    /// </summary>
    /// <remarks></remarks>
    internal class DecompressedBuffer
    {
        internal DecompressedBuffer(byte[] uncompressedData)
        {
            using (var reader = new BinaryReader(new MemoryStream(uncompressedData)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var chunk = new DecompressedChunk(reader);
                    DecompressedChunks.Add(chunk);
                }
            }
        }

        internal DecompressedBuffer(CompressedContainer container)
        {
            foreach (var chunk in container.CompressedChunks)
            {
                DecompressedChunks.Add(new DecompressedChunk(chunk));
            }
        }

        internal List<DecompressedChunk> DecompressedChunks { get; } = new List<DecompressedChunk>();

        internal byte[] Data
        {
            get
            {
                using (var writer = new BinaryWriter(new MemoryStream()))
                {
                    foreach (var chunk in DecompressedChunks)
                    {
                        writer.Write(chunk.Data);
                    }

                    using (var reader = new BinaryReader(writer.BaseStream))
                    {
                        reader.BaseStream.Position = 0;

                        return reader.ReadBytes((int) reader.BaseStream.Length);
                    }
                }
            }
        }
    }
}