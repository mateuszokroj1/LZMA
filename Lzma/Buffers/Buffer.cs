using System.IO;

namespace Lzma.Buffers
{
    internal abstract class Buffer : IBuffer
    {
        #region Constructor

        protected Buffer(uint bufferSize)
        {
            this.buffer = new byte[bufferSize];
        }

        #endregion

        #region Fields

        protected byte[] buffer;
        protected uint position;
        protected ulong processedSize;

        #endregion

        #region Properties

        public uint Length => (uint)this.buffer.Length;

        public ulong ProcessedSize => this.processedSize + this.position;

        public Stream Stream { get; set; }

        #endregion
    }
}
