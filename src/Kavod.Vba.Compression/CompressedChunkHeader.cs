using System;
using System.IO;

namespace Kavod.Vba.Compression
{
    /// <summary>
    /// A CompressedChunkHeader is the first record in a CompressedChunk (section 2.4.1.1.4). A 
    /// CompressedChunkHeader specifies the size of the entire CompressedChunk and the data encoding 
    /// format in CompressedChunk.CompressedData. CompressedChunkHeader information is used by the 
    /// Decompressing a CompressedChunk (section 2.4.1.3.2) and Compressing a DecompressedChunk 
    /// (section 2.4.1.3.7) algorithms.
    /// </summary>
    /// <remarks></remarks>
    internal class CompressedChunkHeader
    {
        internal CompressedChunkHeader(bool compressedFlag, UInt16 chunkSize)
        {
            IsCompressed = compressedFlag;
            CompressedChunkSize = chunkSize;
        }

        internal CompressedChunkHeader(UInt16 header)
        {
            DecodeHeader(header);
        }

        internal CompressedChunkHeader(BinaryReader dataReader)
        {
            var header = dataReader.ReadUInt16();
            DecodeHeader(header);
        }

        private void DecodeHeader(UInt16 header)
        {
            var temp = (UInt16)(header & 0xf000);
            switch (temp)
            {
                case 0xb000:
                    IsCompressed = true;
                    break;

                case 0x3000:
                    IsCompressed = false;
                    break;

                default:
                    throw new Exception();
            }

            // 2.4.1.3.12 Extract CompressedChunkSize
            // SET temp TO Header BITWISE AND 0x0FFF
            // SET Size TO temp PLUS 3
            CompressedChunkSize = (UInt16)((header & 0xfff) + 3);

            ValidateChunkSizeAndCompressedFlag();
        }

        internal bool IsCompressed { get; private set; }

        internal UInt16 CompressedChunkSize { get; private set; }

        internal UInt16 CompressedChunkDataSize => (UInt16)(CompressedChunkSize - 2);

        internal byte[] SerializeData()
        {
            ValidateChunkSizeAndCompressedFlag();

            UInt16 header;
            if (IsCompressed)
            {
                header = (UInt16)(0xb000 | (CompressedChunkSize - 3));
            }
            else
            {
                header = (UInt16)(0x3000 | (CompressedChunkSize - 3));
            }
            return BitConverter.GetBytes(header);
        }

        private void ValidateChunkSizeAndCompressedFlag()
        {
            if (IsCompressed 
                && CompressedChunkSize > 4095)
            {
                throw new Exception();
            }
            if (!IsCompressed 
                && CompressedChunkSize != 4095)
            {
                throw new Exception();
            }
        }
    }
}