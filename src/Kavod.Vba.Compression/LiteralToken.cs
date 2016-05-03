using System;
using System.IO;
using System.Linq;

namespace Kavod.Vba.Compression
{
    /// <summary>
    /// A LiteralToken is a copy of one byte, in uncompressed format, from the DecompressedBuffer 
    /// (section 2.4.1.1.2).
    /// </summary>
    /// <remarks></remarks>
    internal class LiteralToken : IToken, IEquatable<LiteralToken>
    {
        private readonly byte[] _data;

        internal LiteralToken(BinaryReader dataReader)
        {
            _data = dataReader.ReadBytes(1);
        }

        internal LiteralToken(byte data)
        {
            _data = new [] { data };
        }

        public void DecompressToken(BinaryWriter writer)
        {
            writer.Write(_data);
            writer.Flush();
        }

        public byte[] SerializeData()
        {
            return _data;
        }

        public long Length => 1L;

        #region IEquatable
        public static bool operator !=(LiteralToken first, LiteralToken second)
        {
            return !(first == second);
        }

        public static bool operator ==(LiteralToken first, LiteralToken second)
        {
            return Equals(first, second);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LiteralToken);
        }

        public bool Equals(IToken other)
        {
            return Equals(other as LiteralToken);
        }

        public bool Equals(LiteralToken other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            return other._data.SequenceEqual(_data);
        }

        public override int GetHashCode()
        {
            return _data.GetHashCode();
        }
        #endregion
    }
}