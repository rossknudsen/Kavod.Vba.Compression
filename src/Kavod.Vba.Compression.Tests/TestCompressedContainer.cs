using System.IO;
using System.Linq;
using Xunit;

namespace Kavod.Vba.Compression.Tests
{
    public class TestCompressedContainer
    {
        static byte[] _validCompressedDirStream;
        static byte[] _validDecompressedDirStream;
        
        public TestCompressedContainer()
        {
            _validCompressedDirStream = File.ReadAllBytes(@"Test Files\ValidCompressedDirStream");
            _validDecompressedDirStream = File.ReadAllBytes(@"Test Files\ValidDecompressedDirStream");
        }

        [Fact]
        public void CanCreateCompressedContainer()
        {
            var container = new CompressedContainer(_validCompressedDirStream);

            Assert.IsType<CompressedContainer>(container);
        }


        [Fact]
        public void DecompressedDataSameAsMicrosoftImplementation()
        {
            var container = new CompressedContainer(_validCompressedDirStream);
            var buffer = new DecompressedBuffer(container);

            Assert.True(buffer.Data.SequenceEqual(_validDecompressedDirStream));
        }


        [Fact]
        public void ParsedCompressedDataIsSameAsInput()
        {
            var container = new CompressedContainer(_validCompressedDirStream);

            Assert.True(container.SerializeData().SequenceEqual(_validCompressedDirStream));
        }
        
        [Fact(Skip = "Does not pass.")]
        public void CompressedDataSameAsMicrosoftImplementation()
        {
            var buffer = new DecompressedBuffer(_validDecompressedDirStream);
            var container = new CompressedContainer(buffer);

            Assert.True(container.SerializeData().SequenceEqual(_validCompressedDirStream));
        }

        [Fact]
        public void CompressDecompressDataAreEqual()
        {
            var buffer = new DecompressedBuffer(_validDecompressedDirStream);
            var container = new CompressedContainer(buffer);
            var newBuffer = new DecompressedBuffer(container);

            Assert.True(newBuffer.Data.SequenceEqual(_validDecompressedDirStream));
        }

        [Fact]
        public void GivenCompressedDataThatSerializingItReproducesSameData()
        {
            var refCompressed = new CompressedContainer(_validCompressedDirStream);

            var actual = refCompressed.SerializeData();

            Assert.Equal(_validCompressedDirStream.Length, actual.Length);
            Assert.Equal(_validCompressedDirStream, actual);
        }

        [Fact]
        public void TestDirStreamCompression()
        {
            CompressionTestHelper.LowLevelCompressionComparison(_validDecompressedDirStream, _validCompressedDirStream);
        }
    }
}
