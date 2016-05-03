using System;
using Xunit;

namespace Kavod.Vba.Compression.Tests
{
    public class TestCopyToken
    {
        [Fact]
        public void GivenRangeOfPositionOffsetAndLengthPackingThenunPackingDataProducesTheOriginalParameters()
        {
            const int increment = 5;

            for (var position = 2; position < 4096; position = position + increment)
            {
                for (UInt16 offset = 1; offset < position; offset = (ushort)(offset + increment))
                {
                    var result = CopyToken.CopyTokenHelp(position);

                    for (UInt16 length = 3; length <= result.MaximumLength; length = (ushort)(length + increment))
                    {
                        var tokenData = CopyToken.Pack(position, offset, length);

                        UInt16 actualOffset;
                        UInt16 actualLength;
                        CopyToken.UnPack(tokenData, position, out actualOffset, out actualLength);

                        Assert.Equal(offset, actualOffset);
                        Assert.Equal(length, actualLength);
                    }
                }
            }
            
        }

        //  Position        #bits       Max Len         #bits
        //                  Len                         Offset
        //  ==================================================
        //  1 - 16          12          4098            4
        //  17 - 32         11          2050            5
        //  33 - 64         10          1026            6
        //  65 - 128        9           514             7
        //  129 - 256       8           258             8
        //  257 - 512       7           130             9
        //  513 - 1024      6           66              10
        //  1025 - 2048     5           34              11
        //  2049 - 4096     4           18              12
        [Theory]
        [InlineData(1, 12, 4098, 4)]
        [InlineData(16, 12, 4098, 4)]
        [InlineData(17, 11, 2050, 5)]
        [InlineData(32, 11, 2050, 5)]
        [InlineData(33, 10, 1026, 6)]
        [InlineData(64, 10, 1026, 6)]
        [InlineData(65, 9, 514, 7)]
        [InlineData(128, 9, 514, 7)]
        [InlineData(129, 8, 258, 8)]
        [InlineData(256, 8, 258, 8)]
        [InlineData(257, 7, 130, 9)]
        [InlineData(512, 7, 130, 9)]
        [InlineData(513, 6, 66, 10)]
        [InlineData(1024, 6, 66, 10)]
        [InlineData(1025, 5, 34, 11)]
        [InlineData(2048, 5, 34, 11)]
        [InlineData(2049, 4, 18, 12)]
        [InlineData(4096, 4, 18, 12)]
        public void TestTokenHelp(int position, ushort expectedLengthBitCount, 
            ushort expectedMaxLength, ushort expectedOffsetBitCount)
        {
            var result = CopyToken.CopyTokenHelp(position);

            Assert.Equal(expectedLengthBitCount, result.LengthBitCount);
            Assert.Equal(expectedMaxLength, result.MaximumLength);
            Assert.Equal(expectedOffsetBitCount, result.BitCount);
        }
    }
}
