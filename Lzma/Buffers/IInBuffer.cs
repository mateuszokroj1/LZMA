using System.IO;

namespace Lzma.Buffers
{
    internal interface IInBuffer : IBuffer
    {
        #region Methods

        void Init(Stream stream);

        bool ReadBlock();

        bool TryReadBlock();

        bool TryReadByte(out byte b);

        byte ReadByte();

        #endregion
    }
}
