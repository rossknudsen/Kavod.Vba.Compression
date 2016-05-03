namespace Kavod.Vba.Compression
{
    internal class RawChunk : IChunkData
    {
        private readonly byte[] _data;

        public RawChunk(byte[] data)
        {
            _data = data;
        }

        public byte[] SerializeData()
        {
            return _data;
        }

        public int Size => _data.Length;
    }
}
