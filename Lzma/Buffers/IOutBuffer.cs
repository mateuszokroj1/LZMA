namespace Lzma.Buffers
{
    internal interface IOutBuffer : IBuffer
    {
        void Init();

        void WriteByte(byte b);

        void FlushData();
    }
}
