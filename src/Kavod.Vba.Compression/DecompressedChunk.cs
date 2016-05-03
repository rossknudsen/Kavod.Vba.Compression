using System;
using System.IO;

namespace Kavod.Vba.Compression
{
    /// <summary>
    /// A DecompressedChunk is a resizable array of bytes in the DecompressedBuffer 
    /// (section 2.4.1.1.2). The byte array is the data from a CompressedChunk (section 2.4.1.1.4) in 
    /// uncompressed format.
    /// </summary>
    /// <remarks></remarks>
    internal class DecompressedChunk
    {
        internal DecompressedChunk(CompressedChunk compressedChunk)
        {
            if (compressedChunk.Header.IsCompressed)
            {
                // Loop through all the data, get TokenSequences and decompress them.
                using (var writer = new BinaryWriter(new MemoryStream()))
                {
                    var tokens = ((CompressedChunkData)compressedChunk.ChunkData).TokenSequences;
                    foreach (var sequence in tokens)
                    {
                        sequence.Tokens.DecompressTokenSequence(writer);
                    }

                    var stream = (MemoryStream)writer.BaseStream;
                    var decompressedData = stream.GetBuffer();
                    Array.Resize(ref decompressedData, (int)stream.Length);

                    Data = decompressedData;
                }
            }
            else
            {
                Data = compressedChunk.ChunkData.SerializeData();
            }
        }

        internal DecompressedChunk(BinaryReader reader)
        {
            var bytesToRead = reader.BaseStream.Length - reader.BaseStream.Position;

            if (bytesToRead > Globals.MaxBytesPerChunk)
                bytesToRead = Globals.MaxBytesPerChunk;

            Data = reader.ReadBytes((int) bytesToRead);
        }

        internal byte[] Data { get; }
    }
}