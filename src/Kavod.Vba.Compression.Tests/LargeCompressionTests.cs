using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kavod.Vba.Compression.Tests
{
    public class LargeCompressionTests
    {
        [Fact]
        public void GivenLargeByteSequenceWithLowCompressibilityCompressionProducesContainerWithMultipleRawChunks()
        {
            var data = GetLargeByteSequenceWithLowCompressibility().ToArray();

            var container = new CompressedContainer(new DecompressedBuffer(data));

            Assert.True(container.CompressedChunks.Count() > 1);
            Assert.True(container.CompressedChunks.Count(c => c.Header.IsCompressed) <= 1);  // last chunk may be compressed
        }

        [Fact]
        public void GivenLargeByteSequenceWithLowCompressibilityCompressingAndDecompressionProducesSameInput()
        {
            var data = GetLargeByteSequenceWithLowCompressibility().ToArray();

            var compressedData = VbaCompression.Compress(data);
            var convertedData = VbaCompression.Decompress(compressedData);

            Assert.True(data != convertedData);
            Assert.True(data.LongLength == convertedData.LongLength);
            Assert.True(data.SequenceEqual(convertedData));
        }

        private IEnumerable<byte> GetLargeByteSequenceWithLowCompressibility()
        {
            for (byte secondByte = 0; secondByte < byte.MaxValue; secondByte++)
            {
                for (byte firstByte = 0; firstByte < byte.MaxValue; firstByte++)
                {
                    if (firstByte != secondByte)
                    {
                        yield return firstByte;
                        yield return secondByte;
                    }
                }
            }
        }
    }
}
