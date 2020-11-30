using System;

using Lzma.Coders;
using Lzma.Structs;
using Lzma.Windows;

namespace Lzma
{
	public class Decoder : Base
	{
        #region Constructor

        public Decoder()
        {
            dictionarySize = 0xFFFFFFFF;
            for (int i = 0; i < kNumLenToPosStates; i++)
                positionSlotDecoder[i] = new BitTreeDecoder(kNumPosSlotBits);
        }

        #endregion

        #region Fields

        OutWindow outWindow = new OutWindow();
		LzDecoder lzDecoder = new LzDecoder();

		BitDecoder[] isMatchDecoders = new BitDecoder[kNumStates << kNumPosStatesBitsMax];
		BitDecoder[] isRepDecoders = new BitDecoder[kNumStates];
		BitDecoder[] isRepG0Decoders = new BitDecoder[kNumStates];
		BitDecoder[] isRepG1Decoders = new BitDecoder[kNumStates];
		BitDecoder[] isRepG2Decoders = new BitDecoder[kNumStates];
		BitDecoder[] isRep0LongDecoders = new BitDecoder[kNumStates << kNumPosStatesBitsMax];

		BitTreeDecoder[] positionSlotDecoder = new BitTreeDecoder[kNumLenToPosStates];
		BitDecoder[] positionDecoders = new BitDecoder[kNumFullDistances - kEndPosModelIndex];

		BitTreeDecoder positionAlignDecoder = new BitTreeDecoder(kNumAlignBits);

		LengthDecoder lengthDecoder = new LengthDecoder();
		LengthDecoder repLengthDecoder = new LengthDecoder();

		LiteralDecoder literalDecoder = new LiteralDecoder();

		uint dictionarySize;
		uint dictionarySizeCheck;

		uint positionStateMask;

        #endregion

#

        #region Methods

        void SetDictionarySize(uint dictionarySize)
		{
			if (this.dictionarySize != dictionarySize)
			{
				this.dictionarySize = dictionarySize;
				this.dictionarySizeCheck = Math.Max(this.dictionarySize, 1);
				uint blockSize = Math.Max(this.dictionarySizeCheck, 1 << 12);
				this.outWindow.Create(blockSize);
			}
		}

		void SetLiteralProperties(int lp, int lc)
		{
			if (lp > 8)
				throw new InvalidParamException();
			if (lc > 8)
				throw new InvalidParamException();
			literalDecoder.Create(lp, lc);
		}

		void SetPosBitsProperties(int pb)
		{
			if (pb > Base.kNumPosStatesBitsMax)
				throw new InvalidParamException();
			uint numPosStates = (uint)1 << pb;
			lengthDecoder.Create(numPosStates);
			repLengthDecoder.Create(numPosStates);
			positionStateMask = numPosStates - 1;
		}

		bool _solid = false;

		void Init(System.IO.Stream inStream, System.IO.Stream outStream)
		{
			lzDecoder.Init(inStream);
			outWindow.Init(outStream, _solid);

			uint i;
			for (i = 0; i < Base.kNumStates; i++)
			{
				for (uint j = 0; j <= positionStateMask; j++)
				{
					uint index = (i << Base.kNumPosStatesBitsMax) + j;
					isMatchDecoders[index].Init();
					isRep0LongDecoders[index].Init();
				}
				isRepDecoders[i].Init();
				isRepG0Decoders[i].Init();
				isRepG1Decoders[i].Init();
				isRepG2Decoders[i].Init();
			}

			literalDecoder.Init();
			for (i = 0; i < Base.kNumLenToPosStates; i++)
				positionSlotDecoder[i].Init();
			// m_PosSpecDecoder.Init();
			for (i = 0; i < Base.kNumFullDistances - Base.kEndPosModelIndex; i++)
				positionDecoders[i].Init();

			lengthDecoder.Init();
			repLengthDecoder.Init();
			positionAlignDecoder.Init();
		}

		public void Code(System.IO.Stream inStream, System.IO.Stream outStream,
			Int64 inSize, Int64 outSize)
		{
			Init(inStream, outStream);

			var state = new State();
			state.Init();
			uint rep0 = 0, rep1 = 0, rep2 = 0, rep3 = 0;

			UInt64 nowPos64 = 0;
			UInt64 outSize64 = (UInt64)outSize;
			if (nowPos64 < outSize64)
			{
				if (isMatchDecoders[state.Index << Base.kNumPosStatesBitsMax].Decode(lzDecoder) != 0)
					throw new DataErrorException();
				state.UpdateChar();
				byte b = literalDecoder.DecodeNormal(lzDecoder, 0, 0);
				outWindow.PutByte(b);
				nowPos64++;
			}
			while (nowPos64 < outSize64)
			{
				// UInt64 next = Math.Min(nowPos64 + (1 << 18), outSize64);
					// while(nowPos64 < next)
				{
					uint posState = (uint)nowPos64 & positionStateMask;
					if (isMatchDecoders[(state.Index << Base.kNumPosStatesBitsMax) + posState].Decode(lzDecoder) == 0)
					{
						byte b;
						byte prevByte = outWindow.GetByte(0);
						if (!state.IsCharState())
							b = literalDecoder.DecodeWithMatchByte(lzDecoder,
								(uint)nowPos64, prevByte, outWindow.GetByte(rep0));
						else
							b = literalDecoder.DecodeNormal(lzDecoder, (uint)nowPos64, prevByte);
						outWindow.PutByte(b);
						state.UpdateChar();
						nowPos64++;
					}
					else
					{
						uint len;
						if (isRepDecoders[state.Index].Decode(lzDecoder) == 1)
						{
							if (isRepG0Decoders[state.Index].Decode(lzDecoder) == 0)
							{
								if (isRep0LongDecoders[(state.Index << Base.kNumPosStatesBitsMax) + posState].Decode(lzDecoder) == 0)
								{
									state.UpdateShortRep();
									outWindow.PutByte(outWindow.GetByte(rep0));
									nowPos64++;
									continue;
								}
							}
							else
							{
								UInt32 distance;
								if (isRepG1Decoders[state.Index].Decode(lzDecoder) == 0)
								{
									distance = rep1;
								}
								else
								{
									if (isRepG2Decoders[state.Index].Decode(lzDecoder) == 0)
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
							len = repLengthDecoder.Decode(lzDecoder, posState) + Base.kMatchMinLen;
							state.UpdateRep();
						}
						else
						{
							rep3 = rep2;
							rep2 = rep1;
							rep1 = rep0;
							len = Base.kMatchMinLen + lengthDecoder.Decode(lzDecoder, posState);
							state.UpdateMatch();
							uint posSlot = positionSlotDecoder[Base.GetLenToPosState(len)].Decode(lzDecoder);
							if (posSlot >= Base.kStartPosModelIndex)
							{
								int numDirectBits = (int)((posSlot >> 1) - 1);
								rep0 = ((2 | (posSlot & 1)) << numDirectBits);
								if (posSlot < Base.kEndPosModelIndex)
									rep0 += BitTreeDecoder.ReverseDecode(positionDecoders,
											rep0 - posSlot - 1, lzDecoder, numDirectBits);
								else
								{
									rep0 += (lzDecoder.DecodeDirectBits(
										numDirectBits - Base.kNumAlignBits) << Base.kNumAlignBits);
									rep0 += positionAlignDecoder.ReverseDecode(lzDecoder);
								}
							}
							else
								rep0 = posSlot;
						}
						if (rep0 >= outWindow.TrainSize + nowPos64 || rep0 >= dictionarySizeCheck)
						{
							if (rep0 == 0xFFFFFFFF)
								break;
							throw new DataErrorException();
						}
						outWindow.CopyBlock(rep0, len);
						nowPos64 += len;
					}
				}
			}
		}

		public void SetDecoderProperties(byte[] properties)
		{
			if (properties.Length < 5)
				throw new InvalidParamException();
			int lc = properties[0] % 9;
			int remainder = properties[0] / 9;
			int lp = remainder % 5;
			int pb = remainder / 5;
			if (pb > Base.kNumPosStatesBitsMax)
				throw new InvalidParamException();
			UInt32 dictionarySize = 0;
			for (int i = 0; i < 4; i++)
				dictionarySize += ((UInt32)(properties[1 + i])) << (i * 8);
			SetDictionarySize(dictionarySize);
			SetLiteralProperties(lp, lc);
			SetPosBitsProperties(pb);
		}

		public bool Train(System.IO.Stream stream)
		{
			_solid = true;
			return outWindow.Train(stream);
		}

        #endregion
    }
}
