using System.IO;

namespace Lzma.Buffers
{
	internal class InBuffer : Buffer, IInBuffer
	{
        #region Constructor

        public InBuffer(uint bufferSize) : base(bufferSize) { }

        #endregion

        #region Fields

        private uint limit;
		private bool streamWasExhausted;

        #endregion

        public void Init(Stream stream)
		{
			Stream = stream;
			this.processedSize = 0;
			this.limit = 0;
			this.position = 0;
			this.streamWasExhausted = false;
		}

		public bool ReadBlock()
		{
			if (this.streamWasExhausted)
				return false;

			this.processedSize += this.position;
			uint aNumProcessedBytes = (uint)Stream.Read(this.buffer, 0, (int)Length);
			this.position = 0;
			this.limit = aNumProcessedBytes;
			this.streamWasExhausted = aNumProcessedBytes == 0;
			return !this.streamWasExhausted;
		}

        public bool TryReadBlock() => this.position < this.limit && ReadBlock();

        public bool TryReadByte(out byte b)
		{
            if (!TryReadBlock())
            {
                b = 0;
                return false;
            }

			b = this.buffer[this.position++];
			return true;
		}

		public byte ReadByte()
		{
			if (!TryReadBlock())
					return 0xFF; // TODO: replace with throw

			return this.buffer[this.position++];
		}
	}
}
