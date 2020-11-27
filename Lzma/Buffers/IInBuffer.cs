using System;
using System.IO;

namespace Lzma.Buffers
{
    internal interface IInBuffer : IDisposable
    {
        #region Properties

        uint Length { get; }

        ulong ProcessedSize { get; }

        #endregion

        #region Methods

        void Init(Stream stream);

        bool ReadBlock();

        bool TryReadBlock();

        void ReleaseStream();

        bool TryReadByte(out byte b);

        byte ReadByte();

        #endregion
    }
}
