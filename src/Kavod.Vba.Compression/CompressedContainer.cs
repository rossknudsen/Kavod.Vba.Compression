using System;
using System.Collections.Generic;
using System.IO;

namespace Kavod.Vba.Compression
{
    /// <summary>
    /// A CompressedContainer is an array of bytes holding the compressed data. The Decompression 
    /// algorithm (section 2.4.1.3.1) processes a CompressedContainer to populate a DecompressedBuffer. 
    /// The Compression algorithm (section 2.4.1.3.6) processes a DecompressedBuffer to produce a 
    /// CompressedContainer.  A CompressedContainer MUST be the last array of bytes in a stream (1). 
    /// On read, the end of stream (1) indicator determines when the entire CompressedContainer has 
    /// been read.  The CompressedContainer is a SignatureByte followed by array of CompressedChunk 
    /// (section 2.4.1.1.4) structures.
    /// </summary>
    /// <remarks></remarks>
    internal class CompressedContainer
    {
        private const byte SignatureByteSig = 0x1;

        private readonly List<CompressedChunk> _compressedChunks = new List<CompressedChunk>();
        
        internal CompressedContainer(byte[] compressedData)
        {
            var reader = new BinaryReader(new MemoryStream(compressedData));

            if (reader.ReadByte() != SignatureByteSig)
            {
                throw new Exception();
            }

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                _compressedChunks.Add(new CompressedChunk(reader));
            }
        }

        internal CompressedContainer(DecompressedBuffer buffer)
        {
            foreach (var chunk in buffer.DecompressedChunks)
            {
                _compressedChunks.Add(new CompressedChunk(chunk));
            }
        }

        internal IEnumerable<CompressedChunk> CompressedChunks => _compressedChunks;

        internal byte[] SerializeData()
        {
            using (var writer = new BinaryWriter(new MemoryStream()))
            {
                writer.Write(SignatureByteSig);

                foreach (var chunk in CompressedChunks)
                {
                    writer.Write(chunk.SerializeData());
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