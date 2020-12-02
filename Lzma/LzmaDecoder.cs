using System;
using System.IO;

using Lzma.Coders;
using Lzma.Structs;
using Lzma.Windows;

namespace Lzma
{
    /// <summary>
    /// LZMA decoding class.
    /// </summary>
	public class Decoder : Base
	{
        #region Constructor

        public Decoder()
        {
            this.dictionarySize = initDictionarySize;
            for (int i = 0; i < kNumLenToPosStates; i++)
                this.positionSlotDecoder[i] = new BitTreeDecoder(kNumPosSlotBits);
        }

        #endregion

        #region Fields

        private const uint initDictionarySize = 0xFFFFFFFF;

        private readonly OutWindow outWindow = new OutWindow();
        private readonly RangeDecoder rangeDecoder = new RangeDecoder();
        private readonly BitDecoder[] isMatchDecoders = new BitDecoder[kNumStates << kNumPosStatesBitsMax];
        private readonly BitDecoder[] isRepDecoders = new BitDecoder[kNumStates];
        private readonly BitDecoder[] isRepG0Decoders = new BitDecoder[kNumStates];
        private readonly BitDecoder[] isRepG1Decoders = new BitDecoder[kNumStates];
        private readonly BitDecoder[] isRepG2Decoders = new BitDecoder[kNumStates];
        private readonly BitDecoder[] isRep0LongDecoders = new BitDecoder[kNumStates << kNumPosStatesBitsMax];
        private readonly BitTreeDecoder[] positionSlotDecoder = new BitTreeDecoder[kNumLenToPosStates];
        private readonly BitDecoder[] positionDecoders = new BitDecoder[kNumFullDistances - kEndPosModelIndex];
        private readonly BitTreeDecoder positionAlignDecoder = new BitTreeDecoder(kNumAlignBits);
        private readonly LengthDecoder lengthDecoder = new LengthDecoder();
        private readonly LengthDecoder repLengthDecoder = new LengthDecoder();
        private readonly LiteralDecoder literalDecoder = new LiteralDecoder();

		private uint dictionarySize;
		private uint dictionarySizeCheck;

		private uint positionStateMask;

        private bool solid = false;

        #endregion

        #region Methods

        private void SetDictionarySize(uint dictionarySize)
		{
			if (this.dictionarySize != dictionarySize)
			{
				this.dictionarySize = dictionarySize;
				this.dictionarySizeCheck = Math.Max(this.dictionarySize, 1);
				uint blockSize = Math.Max(this.dictionarySizeCheck, 1 << 12);
				this.outWindow.Create(blockSize);
			}
		}

		private void SetLiteralProperties(int lp, int lc)
		{
			if (lp > 8)
				throw new ArgumentOutOfRangeException(nameof(lp));

            if (lc > 8)
                throw new ArgumentOutOfRangeException(nameof(lc));

            this.literalDecoder.Create(lp, lc);
		}

		private void SetPosBitsProperties(int pb)
		{
            if (pb > kNumPosStatesBitsMax)
                throw new ArgumentOutOfRangeException(nameof(pb));

			uint numPosStates = (uint)1 << pb;
            this.lengthDecoder.Create(numPosStates);
            this.repLengthDecoder.Create(numPosStates);
            this.positionStateMask = numPosStates - 1;
		}

		private void Init(Stream inStream, Stream outStream)
		{
            this.rangeDecoder.Init(inStream);
            this.outWindow.Init(outStream, this.solid);

			uint i;

			for (i = 0; i < kNumStates; ++i)
			{
				for (uint j = 0; j <= this.positionStateMask; ++j)
				{
					uint index = (i << kNumPosStatesBitsMax) + j;
                    this.isMatchDecoders[index].Init();
                    this.isRep0LongDecoders[index].Init();
				}

                this.isRepDecoders[i].Init();
                this.isRepG0Decoders[i].Init();
                this.isRepG1Decoders[i].Init();
                this.isRepG2Decoders[i].Init();
			}

            this.literalDecoder.Init();

			for (i = 0; i < kNumLenToPosStates; ++i)
                this.positionSlotDecoder[i].Init();
			
			for (i = 0; i < kNumFullDistances - kEndPosModelIndex; ++i)
                this.positionDecoders[i].Init();

            this.lengthDecoder.Init();
            this.repLengthDecoder.Init();
            this.positionAlignDecoder.Init();
		}

		public void Code(Stream inStream, Stream outStream,
            long inSize, long outSize)
		{
			Init(inStream, outStream);

			var state = new State();
			state.Init();

			uint rep0 = 0, rep1 = 0, rep2 = 0, rep3 = 0;
            ulong nowPos64 = 0;
            ulong outSize64 = (ulong)outSize;

			if (nowPos64 < outSize64)
			{
                if (this.isMatchDecoders[state.Index << kNumPosStatesBitsMax].Decode(this.rangeDecoder) != 0)
                    throw new InvalidDataException();

				state.UpdateChar();
				byte b = this.literalDecoder.DecodeNormal(this.rangeDecoder, 0, 0);
                this.outWindow.PutByte(b);
				nowPos64++;
			}

			while (nowPos64 < outSize64)
			{
				uint posState = (uint)nowPos64 & this.positionStateMask;

				if (this.isMatchDecoders[(state.Index << kNumPosStatesBitsMax) + posState].Decode(this.rangeDecoder) == 0)
				{
					byte b;
					byte prevByte = this.outWindow.GetByte(0);

					if (!state.IsCharState)
						b = this.literalDecoder.DecodeWithMatchByte(this.rangeDecoder,
							(uint)nowPos64, prevByte, this.outWindow.GetByte(rep0));
					else
						b = this.literalDecoder.DecodeNormal(this.rangeDecoder, (uint)nowPos64, prevByte);

                    this.outWindow.PutByte(b);
					state.UpdateChar();
					nowPos64++;
				}
				else
				{
					uint len;

					if (this.isRepDecoders[state.Index].Decode(this.rangeDecoder) == 1)
					{
						if (this.isRepG0Decoders[state.Index].Decode(this.rangeDecoder) == 0)
						{
							if (this.isRep0LongDecoders[(state.Index << kNumPosStatesBitsMax) + posState].Decode(this.rangeDecoder) == 0)
							{
								state.UpdateShortRep();
                                this.outWindow.PutByte(this.outWindow.GetByte(rep0));
								nowPos64++;
								continue;
							}
						}
						else
						{
                            uint distance;

							if (this.isRepG1Decoders[state.Index].Decode(this.rangeDecoder) == 0)
								distance = rep1;
							else
							{
								if (this.isRepG2Decoders[state.Index].Decode(this.rangeDecoder) == 0)
									distance = rep2;
								else
								{
									distance = rep3;
									rep3 = rep2;
								}

								rep2 = rep1;
							}

							rep1 = rep0;
							rep0 = distance;
						}

						len = this.repLengthDecoder.Decode(this.rangeDecoder, posState) + kMatchMinLen;
						state.UpdateRep();
					}
					else
					{
						rep3 = rep2;
						rep2 = rep1;
						rep1 = rep0;
						len = kMatchMinLen + this.lengthDecoder.Decode(this.rangeDecoder, posState);
						state.UpdateMatch();
						uint posSlot = this.positionSlotDecoder[GetLenToPosState(len)].Decode(this.rangeDecoder);

						if (posSlot >= kStartPosModelIndex)
						{
							int numDirectBits = (int)((posSlot >> 1) - 1);
							rep0 = (2 | (posSlot & 1)) << numDirectBits;

							if (posSlot < kEndPosModelIndex)
								rep0 += BitTreeDecoder.ReverseDecode(this.positionDecoders,
										rep0 - posSlot - 1, this.rangeDecoder, numDirectBits);
							else
							{
								rep0 += this.rangeDecoder.DecodeDirectBits(
									numDirectBits - kNumAlignBits) << kNumAlignBits;
								rep0 += this.positionAlignDecoder.ReverseDecode(this.rangeDecoder);
							}
						}
						else
							rep0 = posSlot;
					}
					if (rep0 >= this.outWindow.TrainSize + nowPos64 || rep0 >= this.dictionarySizeCheck)
					{
						if (rep0 == initDictionarySize)
							break;

						throw new InvalidDataException();
					}

                    this.outWindow.CopyBlock(rep0, len);
					nowPos64 += len;
				}
			}
		}

		public void SetDecoderProperties(byte[] properties)
		{
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

			if (properties.Length < 5)
				throw new ArgumentException("Properties length is invalid.");

			int lc = properties[0] % 9;
			int remainder = properties[0] / 9;
			int lp = remainder % 5;
			int pb = remainder / 5;

			if (pb > kNumPosStatesBitsMax)
				throw new ArgumentException("pb is too high.");

            uint dictionarySize = 0;

			for (int i = 0; i < 4; i++)
				dictionarySize += ((uint)properties[1 + i]) << (i * 8);

			SetDictionarySize(dictionarySize);
			SetLiteralProperties(lp, lc);
			SetPosBitsProperties(pb);
		}

		public bool Train(Stream stream)
		{
            this.solid = true;
			return this.outWindow.Train(stream);
		}

        #endregion
    }
}
