namespace Kavod.Vba.Compression
{
    internal static class Globals
    {
        internal const int MaxBytesPerChunk = 4096;
        internal const int NumberOfChunkHeaderBytes = 2;
        internal const byte PaddingByte = 0x0;
    }
}