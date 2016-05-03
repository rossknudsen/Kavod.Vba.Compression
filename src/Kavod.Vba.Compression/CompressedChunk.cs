using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace Kavod.Vba.Compression
{
    /// <summary>
    /// A CompressedChunk is a record that encodes all data from a DecompressedChunk (section 
    /// 2.4.1.1.3) in compressed format. A CompressedChunk has two parts: a CompressedChunkHeader 
    /// (section 2.4.1.1.5) followed by a CompressedChunkData (section 2.4.1.1.6). The number of bytes 
    /// in a CompressedChunk MUST be greater than or equal to 3. The number of bytes in a 
    /// CompressedChunk MUST be less than or equal to 4098.
    /// </summary>
    /// <remarks></remarks>
    internal class CompressedChunk
    {
        internal CompressedChunk(DecompressedChunk decompressedChunk)
        {
            Contract.Requires<ArgumentNullException>(decompressedChunk != null);
            Contract.Ensures(Header != null);
            Contract.Ensures(ChunkData != null);

            var compressedChunkData = new CompressedChunkData(decompressedChunk);
            if (compressedChunkData.Size < Globals.MaxBytesPerChunk)
            {
                ChunkData = compressedChunkData;
                Header = new CompressedChunkHeader(true, 
                    (ushort)(compressedChunkData.Size + Globals.NumberOfChunkHeaderBytes));
            }
            else
            {
                ChunkData = new RawChunk(decompressedChunk.Data);
                Header = new CompressedChunkHeader(false, 
                    (ushort)(ChunkData.Size + Globals.NumberOfChunkHeaderBytes));
            }
        }

        internal CompressedChunk(BinaryReader dataReader)
        {
            Contract.Requires<ArgumentNullException>(dataReader != null);
            Contract.Ensures(Header != null);
            Contract.Ensures(ChunkData != null);

            Header = new CompressedChunkHeader(dataReader);
            ChunkData = new CompressedChunkData(dataReader, Header.CompressedChunkDataSize);
        }

        internal CompressedChunkHeader Header { get; }

        internal IChunkData ChunkData { get; }

        internal byte[] SerializeData()
        {
            var serializedHeader = Header.SerializeData();
            var serializedChunkData = ChunkData.SerializeData();

            var data = serializedHeader.Concat(serializedChunkData);
            if (!Header.IsCompressed)
            {
                var dataLength = serializedHeader.LongLength + serializedChunkData.LongLength;
                var paddingLength = Globals.NumberOfChunkHeaderBytes
                                    + Globals.MaxBytesPerChunk
                                    - dataLength;
                var padding = Enumerable.Repeat(Globals.PaddingByte, (int)paddingLength);
                data = data.Concat(padding);
            }
            return data.ToArray();
        }
    }
}