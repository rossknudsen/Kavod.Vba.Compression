using System;
using System.IO;
using System.Text;

namespace Kavod.Vba.Compression
{
    /// <summary>
    /// CopyToken is a two-byte record interpreted as an unsigned 16-bit integer in little-endian 
    /// order. A CopyToken is a compressed encoding of an array of bytes from a DecompressedChunk 
    /// (section 2.4.1.1.3). The byte array encoded by a CopyToken is a byte-for-byte copy of a byte 
    /// array elsewhere in the same DecompressedChunk, called a CopySequence (section 2.4.1.3.19).  
    /// 
    /// The starting location, in a DecompressedChunk, is determined by the Compressing a Token 
    /// (section 2.4.1.3.9) and the Decompressing a Token (section 2.4.1.3.5) algorithms. Packed into 
    /// the CopyToken is the Offset, the distance, in byte count, to the beginning of the CopySequence. 
    /// Also packed into the CopyToken is the Length, the number of bytes encoded in the CopyToken. 
    /// Length also specifies the count of bytes in the CopySequence. The values encoded in Offset and 
    /// Length are computed by the Matching (section 2.4.1.3.19.4) algorithm.
    /// </summary>
    /// <remarks></remarks>
    internal class CopyToken : IToken, IEquatable<CopyToken>
    {
        private readonly UInt16 _tokenOffset;
        private readonly UInt16 _tokenLength;

        /// <summary>
        /// Constructor used to create a CopyToken when compressing a DecompressedChunk.
        /// </summary>
        /// <param name="tokenPosition">
        /// The start position of the CopyToken decompressed data in the current DecompressedChunk.
        /// </param>
        /// <param name="tokenOffset">
        /// The offset in bytes from the start position in the current DecompressedChunk from which to 
        /// start copying.
        /// </param>
        /// <param name="tokenLength">The number of bytes to copy from the offset.</param>
        /// <remarks></remarks>

        internal CopyToken(long tokenPosition, UInt16 tokenOffset, UInt16 tokenLength)
        {
            Position = tokenPosition;
            _tokenOffset = tokenOffset;
            _tokenLength = tokenLength;
        }

        /// <summary>
        /// Constructor used to create CopyToken instance when reading compressed token from a stream.
        /// </summary>
        /// <param name="dataReader">
        /// A BinaryReader object where the position is located at an encoded CopyToken.
        /// </param>
        /// <remarks></remarks>
        internal CopyToken(BinaryReader dataReader, long position)
        {
            Position = position;
            CopyToken.UnPack(dataReader.ReadUInt16(), Position, out _tokenOffset, out _tokenLength);
        }

        public long Length => _tokenLength;

        internal UInt16 Offset => _tokenOffset;

        internal long Position { get; }

        internal static UInt16 Pack(long position, UInt16 offset, UInt16 length)
        {
            // 2.4.1.3.19.3 Pack CopyToken
            var result = CopyTokenHelp(position);

            if (length > result.MaximumLength)
                throw new Exception();

            //SET temp1 TO Offset MINUS 1
            var temp1 = (UInt16)(offset - 1);

            //SET temp2 TO 16 MINUS BitCount
            var temp2 = (UInt16)(16 - result.BitCount);

            //SET temp3 TO Length MINUS 3
            var temp3 = (UInt16)(length - 3);

            //SET Token TO (temp1 LEFT SHIFT BY temp2) BITWISE OR temp3
            return (UInt16)((temp1 << temp2) | temp3);
        }

        public void DecompressToken(BinaryWriter writer)
        {
            // It is possible that the length is greater than the offset which means we would need to
            // read more bytes than are available.  To handle this we need to read the bytes available
            // (ie Offset amount) and then pad the remaining length with copies of the data read from 
            // the beginning of the buffer.

            var streamPosition = writer.BaseStream.Position;
            var reader = new BinaryReader(writer.BaseStream, Encoding.Unicode, true);
            reader.BaseStream.Position = streamPosition - _tokenOffset;
            var copySequence = reader.ReadBytes(Math.Min(_tokenOffset, _tokenLength));

            Array.Resize(ref copySequence, _tokenLength);

            for (int i = _tokenOffset; i <= _tokenLength - 1; i++)
            {
                var copyByte = copySequence[i % _tokenOffset];
                copySequence[i] = copyByte;
            }

            // Move the position of the underlying stream back to the original position and write the
            // CopySequence.
            writer.BaseStream.Position = streamPosition;
            writer.Write(copySequence);
        }

        internal static void UnPack(UInt16 packedToken, long position, out UInt16 unpackedOffset, out UInt16 unpackedLength)
        {
            // CALL CopyToken Help (section 2.4.1.3.19.1) returning LengthMask, OffsetMask, and BitCount.
            var result = CopyToken.CopyTokenHelp(position);

            // SET Length TO (Token BITWISE AND LengthMask) PLUS 3.
            unpackedLength = (UInt16)((packedToken & result.LengthMask) + 3);

            // SET temp1 TO Token BITWISE AND OffsetMask.
            var temp1 = (UInt16)(packedToken & result.OffsetMask);

            // SET temp2 TO 16 MINUS BitCount.
            var temp2 = (UInt16)(16 - result.BitCount);

            // SET Offset TO (temp1 RIGHT SHIFT BY temp2) PLUS 1.
            unpackedOffset = (UInt16)((temp1 >> temp2) + 1);
        }

        /// <summary>
        /// CopyToken Help derived bit masks are used by the Unpack CopyToken (section 2.4.1.3.19.2) 
        /// and the Pack CopyToken (section 2.4.1.3.19.3) algorithms. CopyToken Help also derives the 
        /// maximum length for a CopySequence (section 2.4.1.3.19) which is used by the Matching 
        /// algorithm (section 2.4.1.3.19.4).
        /// The pseudocode uses the state variables described in State Variables (section 2.4.1.2): 
        /// DecompressedCurrent and DecompressedChunkStart.
        /// </summary>
        internal static CopyTokenHelpResult CopyTokenHelp(long difference)
        {
            var result = new CopyTokenHelpResult();

            // SET BitCount TO the smallest integer that is GREATER THAN OR EQUAL TO LOGARITHM base 2 
            // of difference
            result.BitCount = 0;
            while ((1 << result.BitCount) < difference)
            {
                result.BitCount += 1;
            }

            // The number of bits used to encode Length MUST be greater than or equal to four. The 
            // number of bits used to encode Length MUST be less than or equal to 12
            // SET BitCount TO the maximum of BitCount and 4
            if (result.BitCount < 4)
                result.BitCount = 4;
            if (result.BitCount > 12)
                throw new Exception();

            // SET LengthMask TO 0xFFFF RIGHT SHIFT BY BitCount
            result.LengthMask = (UInt16)(0xffff >> result.BitCount);

            // SET OffsetMask TO BITWISE NOT LengthMask
            result.OffsetMask = (UInt16)(~result.LengthMask);

            // SET MaximumLength TO (0xFFFF RIGHT SHIFT BY BitCount) PLUS 3
            result.MaximumLength = (UInt16)((0xffff >> result.BitCount) + 3);

            return result;
        }

        public byte[] SerializeData()
        {
            var packedData = Pack(Position, _tokenOffset, _tokenLength);
            return BitConverter.GetBytes(packedData);
        }

        #region Nested Classes

        internal struct CopyTokenHelpResult
        {
            internal UInt16 LengthMask { get; set; }
            internal UInt16 OffsetMask { get; set; }
            internal UInt16 BitCount { get; set; }  // offset bit count.
            internal UInt16 MaximumLength { get; set; }
            internal UInt16 LengthBitCount => (UInt16)(16 - BitCount);
        }

        #endregion

        #region IEquatable
        public static bool operator !=(CopyToken first, CopyToken second)
        {
            return !(first == second);
        }

        public static bool operator ==(CopyToken first, CopyToken second)
        {
            return Equals(first, second);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CopyToken);
        }

        public bool Equals(IToken other)
        {
            return Equals(other as CopyToken);
        }

        public bool Equals(CopyToken other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            return other.Position == Position
                   && other.Length == Length
                   && other.Offset == Offset;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ Length.GetHashCode() ^ Offset.GetHashCode();
        }
        #endregion
    }
}