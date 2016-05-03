using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace Kavod.Vba.Compression
{
    /// <summary>
    /// If CompressedChunkHeader.CompressedChunkFlag (section 2.4.1.1.5) is 0b0, CompressedChunkData 
    /// contains an array of CompressedChunkHeader.CompressedChunkSize elements plus 3 bytes of 
    /// uncompressed data.  If CompressedChunkHeader CompressedChunkFlag is 0b1, CompressedChunkData 
    /// contains an array of TokenSequence (section 2.4.1.1.7) elements.
    /// </summary>
    /// <remarks></remarks>
    internal class CompressedChunkData : IChunkData
    {
        private readonly List<TokenSequence> _tokensequences = new List<TokenSequence>();

        internal CompressedChunkData(DecompressedChunk chunk)
        {
            Contract.Requires<ArgumentNullException>(chunk != null);
            
            var tokens = Tokenizer.TokenizeUncompressedData(chunk.Data);
            _tokensequences.AddRange(tokens.ToTokenSequences());
        }

        internal CompressedChunkData(BinaryReader dataReader, UInt16 compressedChunkDataSize)
        {
            var data = dataReader.ReadBytes(compressedChunkDataSize);

            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                var position = 0;
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var sequence = TokenSequence.GetFromCompressedData(reader, position);
                    _tokensequences.Add(sequence);
                    position += (int)sequence.Tokens.Sum(t => t.Length);
                }
            }
        }

        internal IEnumerable<TokenSequence> TokenSequences => _tokensequences;

        public byte[] SerializeData()
        {
            // get data from TokenSequences.
            var data = from t in _tokensequences
                       from d in t.SerializeData()
                       select d;
            return data.ToArray();
        }

        // TODO this is probably really inefficient.
        public int Size => SerializeData().Length;
    }
}