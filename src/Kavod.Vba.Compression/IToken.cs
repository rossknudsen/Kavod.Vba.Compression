using System;
using System.IO;

namespace Kavod.Vba.Compression
{
    internal interface IToken : IEquatable<IToken>
    {
        void DecompressToken(BinaryWriter writer);

        byte[] SerializeData();

        long Length { get; }
    }
}