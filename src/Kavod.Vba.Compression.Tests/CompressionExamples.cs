using System;
using System.Linq;
using Xunit;

namespace Kavod.Vba.Compression.Tests
{
    public class NoCompression
    {
        // [MS-OVBA] 3.2.1 No Compression Example
        private const UInt16 CodePage = 1252;
        private const string CompressionInputText = "abcdefghijklmnopqrstuv.";
        private const string ExpectedCompressedOutput =
            "01 19 B0 00 61 62 63 64 65 66 67 68 00 69 6A 6B 6C 6D 6E 6F 70 00 71 72 73 74 75 76 2E";
        private const string ExpectedDecompressedOutput =
            "61 62 63 64 65 66 67 68 69 6A 6B 6C 6D 6E 6F 70 71 72 73 74 75 76 2E";

        private static byte[] _expectedCompressedBytes;
        private static byte[] _expectedDecompressedBytes;
        private static byte[] _compressionInputBytes;

        public NoCompression()
        {
            _compressionInputBytes = CompressionInputText.ToMcbsBytes(CodePage);
            _expectedDecompressedBytes = Extensions.StringToByteArray(ExpectedDecompressedOutput.Replace(" ", ""));
            _expectedCompressedBytes = Extensions.StringToByteArray(ExpectedCompressedOutput.Replace(" ", ""));
        }

        [Fact]
        public void InputValueAndDecompressedOutputAreSame()
        {
            Assert.True(_expectedDecompressedBytes.SequenceEqual(_compressionInputBytes));
        }

        [Fact]
        public void CompressionProducesExpectedOutput()
        {
            var compressed = VbaCompression.Compress(_compressionInputBytes);

            Assert.Equal(_expectedCompressedBytes.Length, compressed.Length);
            Assert.True(_expectedCompressedBytes.SequenceEqual(compressed));
        }

        [Fact]
        public void DecompressionProducesExpectedOutput()
        {
            var decompressed = VbaCompression.Decompress(_expectedCompressedBytes);

            Assert.Equal(_expectedDecompressedBytes.Length, _compressionInputBytes.Length);
            Assert.True(_expectedDecompressedBytes.SequenceEqual(_compressionInputBytes));
        }
    }

    public class NormalCompression
    {
        // [MS-OVBA] 3.2.1 No Compression Example
        private const UInt16 CodePage = 1252;
        private const string CompressionInputText = "#aaabcdefaaaaghijaaaaaklaaamnopqaaaaaaaaaaaarstuvwxyzaaa";
        private const string ExpectedCompressedOutput =
            "01 2F B0 00 23 61 61 61 62 63 64 65 82 66 00 70 61 67 68 69 6A 01 38 08 61 6B 6C 00 30 6D 6E 6F" +
            "70 06 71 02 70 04 10 72 73 74 75 76 10 77 78 79 7A 00 3C";
        private const string ExpectedDecompressedOutput =
            "23 61 61 61 62 63 64 65 66 61 61 61 61 67 68 69 6a 61 61 61 61 61 6B 6C 61 61 61 6D 6E 6F 70 71" +
            "61 61 61 61 61 61 61 61 61 61 61 61 72 73 74 75 76 77 78 79 7A 61 61 61";

        private static byte[] _expectedCompressedBytes;
        private static byte[] _expectedDecompressedBytes;
        private static byte[] _compressionInputBytes;
        
        public NormalCompression()
        {
            _compressionInputBytes = CompressionInputText.ToMcbsBytes(CodePage);
            _expectedDecompressedBytes = Extensions.StringToByteArray(ExpectedDecompressedOutput.Replace(" ", ""));
            _expectedCompressedBytes = Extensions.StringToByteArray(ExpectedCompressedOutput.Replace(" ", ""));
        }

        [Fact]
        public void InputValueAndDecompressedOutputAreSame()
        {
            Assert.True(_expectedDecompressedBytes.SequenceEqual(_compressionInputBytes));
        }

        [Fact]
        public void CompressionProducesExpectedOutput()
        {
            CompressionTestHelper.LowLevelCompressionComparison(_expectedDecompressedBytes, _expectedCompressedBytes);
        }

        [Fact]
        public void DecompressionProducesExpectedOutput()
        {
            var decompressed = VbaCompression.Decompress(_expectedCompressedBytes);

            Assert.Equal(_expectedDecompressedBytes.Length, decompressed.Length);
            Assert.True(_expectedDecompressedBytes.SequenceEqual(decompressed));
        }
    }

    public class MaximumCompression
    {
        // [MS-OVBA] 3.2.1 No Compression Example
        private const UInt16 CodePage = 1252;
        private const string CompressionInputText = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private const string ExpectedCompressedOutput = "01 03 B0 02 61 45 00";
        private const string ExpectedDecompressedOutput =
            "61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61" +
            "61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61 61" +
            "61 61 61 61 61 61 61 61 61";

        private static byte[] _expectedCompressedBytes;
        private static byte[] _expectedDecompressedBytes;
        private static byte[] _compressionInputBytes;
        
        public MaximumCompression()
        {
            _compressionInputBytes = CompressionInputText.ToMcbsBytes(CodePage);
            _expectedDecompressedBytes = Extensions.StringToByteArray(ExpectedDecompressedOutput.Replace(" ", ""));
            _expectedCompressedBytes = Extensions.StringToByteArray(ExpectedCompressedOutput.Replace(" ", ""));
        }

        [Fact]
        public void InputValueAndDecompressedOutputAreSame()
        {
            Assert.True(_expectedDecompressedBytes.SequenceEqual(_compressionInputBytes));
        }

        [Fact]
        public void CompressionProducesExpectedOutput()
        {
            var compressed = VbaCompression.Compress(_compressionInputBytes);

            Assert.Equal(_expectedCompressedBytes.Length, compressed.Length);
            Assert.True(_expectedCompressedBytes.SequenceEqual(compressed));
        }

        [Fact]
        public void DecompressionProducesExpectedOutput()
        {
            var decompressed = VbaCompression.Decompress(_expectedCompressedBytes);

            Assert.Equal(_expectedDecompressedBytes.Length, _compressionInputBytes.Length);
            Assert.True(_expectedDecompressedBytes.SequenceEqual(_compressionInputBytes));
        }
    }
}