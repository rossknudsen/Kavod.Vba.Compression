using System;
using System.Linq;
using Xunit;

namespace Kavod.Vba.Compression.Tests
{
    public class TestCompressedChunkHeader
    {
        [Fact]
        public void DecodeEncodeHeader()
        {
            var data = BitConverter.ToUInt16(new byte[] {
            0x50,
            0xb2
            }, 0);
            var header = new CompressedChunkHeader(data);

            Assert.True(BitConverter.GetBytes(data).SequenceEqual(header.SerializeData()));
        }
    }
}
