using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace Kavod.Vba.Compression
{
    /// <summary>
    /// A TokenSequence is a FlagByte followed by an array of Tokens. The number of Tokens in the final 
    /// TokenSequence MUST be greater than or equal to 1. The number of Tokens in the final 
    /// TokenSequence MUST less than or equal to eight. All other TokenSequences in the 
    /// CompressedChunkData MUST contain eight Tokens.
    /// </summary>
    /// <remarks></remarks>
    internal class TokenSequence
    {
        private byte _flagByte;
        private readonly List<IToken> _tokens = new List<IToken>();

        public TokenSequence(IEnumerable<IToken> enumerable) : this()
        {
            _tokens.AddRange(enumerable);
            
            Contract.Assert(_tokens.Count > 0);
            Contract.Assert(_tokens.Count <= 8);
            
            // set the flag byte.
            for (var i = 0; i < _tokens.Count; i++)
            {
                if (_tokens[i] is CopyToken)
                {
                    SetIsCopyToken(i, true);
                }
            }
        }

        private TokenSequence()
        { }

        internal long Length => Tokens.Sum(t => t.Length);

        internal IReadOnlyList<IToken> Tokens => _tokens;

        internal static TokenSequence GetFromCompressedData(BinaryReader reader, long position)
        {
            var sequence = new TokenSequence
            {
                _flagByte = reader.ReadByte()
            };

            for (var i = 0; i <= 7; i++)
            {
                if (sequence.GetIsCopyToken(i))
                {
                    var token = new CopyToken(reader, position);
                    sequence._tokens.Add(token);
                    position += Convert.ToInt64(token.Length);
                }
                else
                {
                    sequence._tokens.Add(new LiteralToken(reader));
                    position += 1;
                }
            }
            return sequence;
        }

        private void SetIsCopyToken(int index, bool value)
        {
            var setByte = (byte)Math.Pow(2, index);
            _flagByte = (byte)(_flagByte | setByte);
        }

        private bool GetIsCopyToken(int index)
        {
            var compareByte = (byte)Math.Pow(2, index);
            return (compareByte & _flagByte) != 0x0;
        }

        internal byte[] SerializeData()
        {
            var data = Enumerable.Repeat(_flagByte, 1);
            foreach (var token in Tokens)
            {
                data = data.Concat(token.SerializeData());
            }
            return data.ToArray();
        }
    }
}