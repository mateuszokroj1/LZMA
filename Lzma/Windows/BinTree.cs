using System;

using Lzma.Helpers;

namespace Lzma.Windows
{
	internal class BinTree : InWindow
	{
        #region Fields

        private uint cyclicBufferPosition;
		private uint cyclicBufferSize;
		private uint matchMaxLength;
        
		private uint[] sons;
		private uint[] hashes;

		private uint hashMask;
		private uint hashSizeSum = 0;
         
		private bool isHashArray = true;

		const uint kHash2Size = 1 << 10;
		const uint kHash3Size = 1 << 16;
		const uint kBT2HashSize = 1 << 16;
		const uint kStartMaxLen = 1;
		const uint kHash3Offset = kHash2Size;
		const uint kEmptyHashValue = 0;
		const uint kMaxValForNormalize = ((uint)1 << 31) - 1;
	
		private uint kNumHashDirectBytes = 0;
		private uint kMinMatchCheck = 4;
		private uint kFixHashSize = kHash2Size + kHash3Size;

        #endregion

        #region Properties

        public uint CutValue { get; set; } = 0xFF;

        #endregion

        #region Methods

        public void SetType(int numHashBytes)
		{
			this.isHashArray = numHashBytes > 2;
			if (this.isHashArray)
			{
				this.kNumHashDirectBytes = 0;
				this.kMinMatchCheck = 4;
				this.kFixHashSize = kHash2Size + kHash3Size;
			}
			else
			{
				this.kNumHashDirectBytes = 2;
				this.kMinMatchCheck = 2 + 1;
				this.kFixHashSize = 0;
			}
		}
		
		public new void Init()
		{
			base.Init();

			for (uint i = 0; i < hashSizeSum; i++)
				this.hashes[i] = kEmptyHashValue;

			this.cyclicBufferPosition = 0;

			ReduceOffsets(-1);
		}

		public new void MovePosition()
		{
			if (++this.cyclicBufferPosition >= this.cyclicBufferSize)
				cyclicBufferPosition = 0;

			base.MovePosition();

			if (Position == kMaxValForNormalize)
				Normalize();
		}

        public void Create(uint historySize, uint keepAddBufferBefore,
                uint matchMaxLength, uint keepAddBufferAfter)
        {
            if (historySize > kMaxValForNormalize - 256)
                throw new ArgumentOutOfRangeException("History size greather than max.");

            CutValue = 16 + (matchMaxLength >> 1);

            uint windowReservSize = (historySize + keepAddBufferBefore +
                    matchMaxLength + keepAddBufferAfter) / 2 + 256;

            Create(historySize + keepAddBufferBefore, matchMaxLength + keepAddBufferAfter, windowReservSize);

            this.matchMaxLength = matchMaxLength;

            uint cyclicBufferSize = historySize + 1;

            if (this.cyclicBufferSize != cyclicBufferSize)
            {
                this.cyclicBufferSize = cyclicBufferSize;
                this.sons = new uint[cyclicBufferSize * 2];
            }

			uint hashSize = kBT2HashSize;

			if (this.isHashArray)
			{
				hashSize = historySize - 1;
				hashSize |= hashSize >> 1;
				hashSize |= hashSize >> 2;
				hashSize |= hashSize >> 4;
				hashSize |= hashSize >> 8;
				hashSize >>= 1;
				hashSize |= 0xFFFF;

				if (hashSize > 1 << 24)
					hashSize >>= 1;

				hashMask = hashSize;
				hashSize++;
				hashSize += kFixHashSize;
			}

			if (hashSize != hashSizeSum)
				hashes = new uint[hashSizeSum = hashSize];
		}

		public uint GetMatches(uint[] distances)
		{
			uint lengthLimit;
			if (Position + this.matchMaxLength <= StreamPosition)
				lengthLimit = this.matchMaxLength;
			else
			{
				lengthLimit = StreamPosition - Position;

				if (lengthLimit < this.kMinMatchCheck)
				{
					MovePosition();
					return 0;
				}
			}

			uint offset = 0;
			uint matchMinPosition = (Position > cyclicBufferSize) ? (Position - cyclicBufferSize) : 0;
			uint cur = BufferOffset + Position;
			uint maxLength = kStartMaxLen;
			uint hashValue, hash2Value = 0, hash3Value = 0;

			if (this.isHashArray)
			{
				uint temp = Crc.Table[this.buffer[cur]] ^ this.buffer[cur + 1];
				hash2Value = temp & (kHash2Size - 1);
				temp ^= (uint)(this.buffer[cur + 2] << 8);
				hash3Value = temp & (kHash3Size - 1);
				hashValue = (temp ^ (Crc.Table[this.buffer[cur + 3]] << 5)) & this.hashMask;
			}
			else
				hashValue = this.buffer[cur] ^ ((uint)this.buffer[cur + 1] << 8);

			uint curMatch = this.hashes[kFixHashSize + hashValue];

			if (this.isHashArray)
			{
				uint curMatch2 = this.hashes[hash2Value];
				uint curMatch3 = this.hashes[kHash3Offset + hash3Value];
				this.hashes[hash2Value] = Position;
				this.hashes[kHash3Offset + hash3Value] = Position;

				if (curMatch2 > matchMinPosition)
					if (this.buffer[BufferOffset + curMatch2] == this.buffer[cur])
					{
						distances[offset++] = maxLength = 2;
						distances[offset++] = Position - curMatch2 - 1;
					}

				if (curMatch3 > matchMinPosition)
					if (this.buffer[BufferOffset + curMatch3] == this.buffer[cur])
					{
						if (curMatch3 == curMatch2)
							offset -= 2;

						distances[offset++] = maxLength = 3;
						distances[offset++] = Position - curMatch3 - 1;
						curMatch2 = curMatch3;
					}

				if (offset != 0 && curMatch2 == curMatch)
				{
					offset -= 2;
					maxLength = kStartMaxLen;
				}
			}

			this.hashes[kFixHashSize + hashValue] = Position;

			uint pointer0 = (this.cyclicBufferPosition << 1) + 1;
			uint pointer1 = this.cyclicBufferPosition << 1;

			uint length0, length1;
			length0 = length1 = this.kNumHashDirectBytes;
			
			if (this.kNumHashDirectBytes != 0)
			{
				if (curMatch > matchMinPosition)
				{
					if (this.buffer[BufferOffset + curMatch + this.kNumHashDirectBytes] !=
							this.buffer[cur + this.kNumHashDirectBytes])
					{
						distances[offset++] = maxLength = this.kNumHashDirectBytes;
						distances[offset++] = Position - curMatch - 1;
					}
				}
			}
			
			uint count = CutValue;
			
			while(true)
			{
				if(curMatch <= matchMinPosition || count-- == 0)
				{
					this.sons[pointer0] = this.sons[pointer1] = kEmptyHashValue;
					break;
				}
				uint delta = Position - curMatch;
				uint cyclicPosition = ((delta <= this.cyclicBufferPosition) ?
							(this.cyclicBufferPosition - delta) :
							(this.cyclicBufferPosition - delta + this.cyclicBufferSize)) << 1;

				uint pby1 = BufferOffset + curMatch;
				uint length = Math.Min(length0, length1);

				if (this.buffer[pby1 + length] == this.buffer[cur + length])
				{
					while(++length != lengthLimit)
						if (this.buffer[pby1 + length] != this.buffer[cur + length])
							break;

					if (maxLength < length)
					{
						distances[offset++] = maxLength = length;
						distances[offset++] = delta - 1;

						if (length == lengthLimit)
						{
                            this.sons[pointer1] = this.sons[cyclicPosition];
                            this.sons[pointer0] = this.sons[cyclicPosition + 1];
							break;
						}
					}
				}

				if (this.buffer[pby1 + length] < this.buffer[cur + length])
				{
                    this.sons[pointer1] = curMatch;
					pointer1 = cyclicPosition + 1;
					curMatch = this.sons[pointer1];
					length1 = length;
				}
				else
				{
					this.sons[pointer0] = curMatch;
					pointer0 = cyclicPosition;
					curMatch = this.sons[pointer0];
					length0 = length;
				}
			}

			MovePosition();
			return offset;
		}

		public void Skip(uint num)
		{
			do
			{
				uint lengthLimit;
				if (Position + matchMaxLength <= StreamPosition)
					lengthLimit = this.matchMaxLength;
				else
				{
					lengthLimit = StreamPosition - Position;
					if (lengthLimit < this.kMinMatchCheck)
					{
						MovePosition();
						continue;
					}
				}

				uint matchMinPosition = (Position > this.cyclicBufferSize) ? (Position - this.cyclicBufferSize) : 0;
				uint cur = BufferOffset + Position;

				uint hashValue;

				if (this.isHashArray)
				{
					uint temp = Crc.Table[this.buffer[cur]] ^ this.buffer[cur + 1];
					uint hash2Value = temp & (kHash2Size - 1);
                    this.hashes[hash2Value] = Position;
					temp ^= (uint)(this.buffer[cur + 2]) << 8;
					uint hash3Value = temp & (kHash3Size - 1);
                    this.hashes[kHash3Offset + hash3Value] = Position;
					hashValue = (temp ^ (Crc.Table[this.buffer[cur + 3]] << 5)) & this.hashMask;
				}
				else
					hashValue = this.buffer[cur] ^ ((uint)(this.buffer[cur + 1]) << 8);

				uint curMatch = this.hashes[this.kFixHashSize + hashValue];
				this.hashes[this.kFixHashSize + hashValue] = Position;

				uint pointer0 = (this.cyclicBufferPosition << 1) + 1;
				uint pointer1 = this.cyclicBufferPosition << 1;

				uint length0, length1;
				length0 = length1 = this.kNumHashDirectBytes;

				uint count = CutValue;

				while (true)
				{
					if (curMatch <= matchMinPosition || count-- == 0)
					{
						this.sons[pointer0] = this.sons[pointer1] = kEmptyHashValue;
						break;
					}

					uint delta = Position - curMatch;
					uint cyclicPosition = ((delta <= cyclicBufferPosition) ?
								(this.cyclicBufferPosition - delta) :
								(this.cyclicBufferPosition - delta + this.cyclicBufferSize)) << 1;

					uint pby1 = BufferOffset + curMatch;
					uint length = Math.Min(length0, length1);
					if (this.buffer[pby1 + length] == this.buffer[cur + length])
					{
						while (++length != lengthLimit)
							if (this.buffer[pby1 + length] != this.buffer[cur + length])
								break;

						if (length == lengthLimit)
						{
							this.sons[pointer1] = this.sons[cyclicPosition];
							this.sons[pointer0] = this.sons[cyclicPosition + 1];
							break;
						}
					}

					if (this.buffer[pby1 + length] < this.buffer[cur + length])
					{
						this.sons[pointer1] = curMatch;
						pointer1 = cyclicPosition + 1;
						curMatch = this.sons[pointer1];
						length1 = length;
					}
					else
					{
						this.sons[pointer0] = curMatch;
						pointer0 = cyclicPosition;
						curMatch = this.sons[pointer0];
						length0 = length;
					}
				}
				MovePosition();
			}
			while (--num != 0);
		}

		private static void NormalizeLinks(uint[] items, uint numItems, uint subValue)
		{
			for (uint i = 0; i < numItems; i++)
			{
				uint value = items[i];

				if (value <= subValue)
					value = kEmptyHashValue;
				else
					value -= subValue;

				items[i] = value;
			}
		}

		private void Normalize()
		{
			uint subValue = Position - this.cyclicBufferSize;
			NormalizeLinks(this.sons, this.cyclicBufferSize * 2, subValue);
			NormalizeLinks(this.hashes, this.hashSizeSum, subValue);
			ReduceOffsets((int)subValue);
		}

        #endregion
    }
}
