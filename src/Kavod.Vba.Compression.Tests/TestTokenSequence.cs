using System;
using Xunit;

namespace Kavod.Vba.Compression.Tests
{
    public class TestTokenSequence
    {
        const int TokenIndexToCheck = 3;

        [Fact]
        public void TestMatchMethod()
        {
            var bytes = new byte[] { 1, 1, 1, 2, 1, 1, 1, 2, 1, 2 };

            UInt16 offset = 0;
            UInt16 length = 0;
            
            Tokenizer.Match(bytes, 4, out offset, out length);

            Assert.Equal(Convert.ToUInt16(4), offset);
            Assert.Equal(Convert.ToUInt16(5), length);
        }
    }
}
