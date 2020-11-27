using System;
using System.IO;

namespace Lzma.Windows
{
	internal class InWindow
	{
        #region Fields

        private byte[] buffer;
        private Stream stream;
        private uint positionLimit;
        private bool streamEndWasReached;
        private uint lastSafePosition;

        #endregion

        #region Properties

        public uint BufferOffset { get; set; }

        public uint BlockSize { get; set; }

        public uint Position { get; set; }

        public uint KeepSizeBefore { get; set; }

        public uint KeepSizeAfter { get; set; }

        public uint StreamPosition { get; set; }

        public uint AvailableBytes => StreamPosition - Position;

        #endregion

        #region Methods

        public void MoveBlock()
		{
			uint offset = BufferOffset + Position - KeepSizeBefore;

			if (offset > 0)
				offset--;
			
			uint numBytes = BufferOffset + StreamPosition - offset;

			for (uint i = 0; i < numBytes; i++)
				this.buffer[i] = this.buffer[offset + i];

			BufferOffset -= offset;
		}

		public virtual void ReadBlock()
		{
			if (this.streamEndWasReached)
				return;

			while (true)
			{
				int size = (int)(-BufferOffset + BlockSize - StreamPosition);

				if (size == 0)
					return;

				int numReadBytes = this.stream.Read(this.buffer, (int)(BufferOffset + StreamPosition), size);

				if (numReadBytes == 0)
				{
					this.positionLimit = StreamPosition;
					uint pointerToPostion = BufferOffset + this.positionLimit;

					if (pointerToPostion > this.lastSafePosition)
						this.positionLimit = this.lastSafePosition - BufferOffset;

					this.streamEndWasReached = true;
					return;
				}

				StreamPosition += (uint)numReadBytes;

				if (StreamPosition >= Position + KeepSizeAfter)
					this.positionLimit = StreamPosition - KeepSizeAfter;
			}
		}

		public void Create(uint keepSizeBefore, uint keepSizeAfter, uint keepSizeReserv)
		{
			KeepSizeBefore = keepSizeBefore;
			KeepSizeAfter = keepSizeAfter;
			uint blockSize = keepSizeBefore + keepSizeAfter + keepSizeReserv;

			if (this.buffer == null || BlockSize != blockSize)
			{
				BlockSize = blockSize;
				this.buffer = new byte[BlockSize];
			}

			this.lastSafePosition = BlockSize - KeepSizeAfter;
		}

		public void Init()
		{
			BufferOffset = 0;
			Position = 0;
			StreamPosition = 0;
			this.streamEndWasReached = false;
			ReadBlock();
		}

		public void MovePosition()
		{
			Position++;

			if (Position > this.positionLimit)
			{
				uint pointerToPosition = BufferOffset + Position;

				if (pointerToPosition > this.lastSafePosition)
					MoveBlock();

				ReadBlock();
			}
		}

		public byte this[int index] => this.buffer[BufferOffset + Position + index];

		public uint GetMatchLength(int index, uint distance, uint limit)
		{
			if (this.streamEndWasReached && Position + index + limit > StreamPosition)
					limit = StreamPosition - (uint)(Position + index);

			distance++;

			uint pby = BufferOffset + Position + (uint)index;

			uint i;
			for (i = 0;
                i < limit && this.buffer[pby + i] == this.buffer[pby + i - distance];
                i++
            );

			return i;
		}

		public void ReduceOffsets(uint subValue)
		{
			BufferOffset += subValue;
			this.positionLimit -= subValue;
			Position -= subValue;
			StreamPosition -= subValue;
		}

        #endregion
    }
}
