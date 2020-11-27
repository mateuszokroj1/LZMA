using System.IO;

namespace Lzma.Buffers
{
	internal class OutBuffer : Buffer, IOutBuffer
	{
        #region Constructor

        public OutBuffer(uint bufferSize) : base(bufferSize) { }

        #endregion

        #region Methods

        public void Init()
		{
			this.processedSize = 0;
			this.position = 0;
		}

		public void WriteByte(byte b)
		{
			this.buffer[this.position++] = b;

			if (this.position >= Length)
				FlushData();
		}

		public void FlushData()
		{
			if (position == 0)
				return;

			Stream.Write(this.buffer, 0, (int)this.position);
			this.position = 0;
		}

        #endregion
    }
}
