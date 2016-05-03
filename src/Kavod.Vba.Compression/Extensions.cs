using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Kavod.Vba.Compression
{
    internal static class Extensions
    {
        [DebuggerStepThrough]
        internal static byte[] ToMcbsBytes(this string textToConvert, UInt16 codePage)
        {
            return Encoding.GetEncoding(codePage).GetBytes(textToConvert);
        }

        // http://stackoverflow.com/questions/321370/convert-hex-string-to-byte-array
        internal static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
