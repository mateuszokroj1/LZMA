using System.IO;

namespace Lzma.Windows
{
	internal class OutWindow
	{
        #region Fields

        private byte[] buffer = null;
		private uint position;
		private uint windowSize = 0;
		private uint streamPosition;
		private Stream stream;

        #endregion

        #region Properties

        public uint TrainSize { get; set; }

        #endregion

        public void Create(uint windowSize)
		{
			if (this.windowSize != windowSize)
				this.buffer = new byte[windowSize];

			this.windowSize = windowSize;
			this.position = 0;
			this.streamPosition = 0;
		}

		public void Init(Stream stream, bool solid)
		{
			this.stream = stream;

			if (!solid)
			{
				this.streamPosition = 0;
				this.position = 0;
				TrainSize = 0;
			}
		}
	
		public bool Train(Stream stream)
		{
			long len = stream.Length;
			uint size = (len < this.windowSize) ? (uint)len : this.windowSize;
			TrainSize = size;
			stream.Position = len - size;
			this.streamPosition = this.position = 0;

			while (size > 0)
			{
				uint curSize = this.windowSize - this.position;
				if (size < curSize)
					curSize = size;
				int numReadBytes = stream.Read(this.buffer, (int)this.position, (int)curSize);
				if (numReadBytes == 0)
					return false;
				size -= (uint)numReadBytes;
				this.position += (uint)numReadBytes;
				this.streamPosition += (uint)numReadBytes;
				if (this.position == this.windowSize)
					this.streamPosition = this.position = 0;
			}

			return true;
		}

		public void Flush()
		{
			uint size = this.position - this.streamPosition;

			if (size == 0)
				return;

			this.stream.Write(this.buffer, (int)this.streamPosition, (int)size);

			if (this.position >= this.windowSize)
				this.position = 0;

			this.streamPosition = this.position;
		}

		public void CopyBlock(uint distance, uint length)
		{
			uint localPosition = this.position - distance - 1;

			if (localPosition >= this.windowSize)
				localPosition += this.windowSize;

			for (; length > 0; length--)
			{
				if (localPosition >= this.windowSize)
					localPosition = 0;

				this.buffer[this.position++] = this.buffer[localPosition++];

				if (this.position >= this.windowSize)
					Flush();
			}
		}

		public void PutByte(byte b)
		{
			this.buffer[this.position++] = b;

			if (this.position >= this.windowSize)
				Flush();
		}

		public byte GetByte(uint distance)
		{
			uint localPosition = this.position - distance - 1;

			if (localPosition >= this.windowSize)
				localPosition += this.windowSize;

			return this.buffer[localPosition];
		}
	}
}
