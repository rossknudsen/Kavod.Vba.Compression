namespace Kavod.Vba.Compression
{
    internal interface IChunkData
    {
        byte[] SerializeData();

        int Size { get; }
    }
}