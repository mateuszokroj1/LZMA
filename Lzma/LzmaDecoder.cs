using System;

using Lzma.Coders;
using Lzma.Structs;
using Lzma.Windows;

namespace Lzma
{
	public class Decoder
	{
		

		

		OutWindow m_OutWindow = new OutWindow();
		Coders.Decoder m_RangeDecoder = new Coders.Decoder();

		BitDecoder[] m_IsMatchDecoders = new BitDecoder[Base.kNumStates << Base.kNumPosStatesBitsMax];
		BitDecoder[] m_IsRepDecoders = new BitDecoder[Base.kNumStates];
		BitDecoder[] m_IsRepG0Decoders = new BitDecoder[Base.kNumStates];
		BitDecoder[] m_IsRepG1Decoders = new BitDecoder[Base.kNumStates];
		BitDecoder[] m_IsRepG2Decoders = new BitDecoder[Base.kNumStates];
		BitDecoder[] m_IsRep0LongDecoders = new BitDecoder[Base.kNumStates << Base.kNumPosStatesBitsMax];

		BitTreeDecoder[] m_PosSlotDecoder = new BitTreeDecoder[Base.kNumLenToPosStates];
		BitDecoder[] m_PosDecoders = new BitDecoder[Base.kNumFullDistances - Base.kEndPosModelIndex];

		BitTreeDecoder m_PosAlignDecoder = new BitTreeDecoder(Base.kNumAlignBits);

		LengthDecoder m_LenDecoder = new LengthDecoder();
		LengthDecoder m_RepLenDecoder = new LengthDecoder();

		LiteralDecoder m_LiteralDecoder = new LiteralDecoder();

		uint m_DictionarySize;
		uint m_DictionarySizeCheck;

		uint m_PosStateMask;

		public Decoder()
		{
			m_DictionarySize = 0xFFFFFFFF;
			for (int i = 0; i < Base.kNumLenToPosStates; i++)
				m_PosSlotDecoder[i] = new BitTreeDecoder(Base.kNumPosSlotBits);
		}

		void SetDictionarySize(uint dictionarySize)
		{
			if (m_DictionarySize != dictionarySize)
			{
				m_DictionarySize = dictionarySize;
				m_DictionarySizeCheck = Math.Max(m_DictionarySize, 1);
				uint blockSize = Math.Max(m_DictionarySizeCheck, (1 << 12));
				m_OutWindow.Create(blockSize);
			}
		}

		void SetLiteralProperties(int lp, int lc)
		{
			if (lp > 8)
				throw new InvalidParamException();
			if (lc > 8)
				throw new InvalidParamException();
			m_LiteralDecoder.Create(lp, lc);
		}

		void SetPosBitsProperties(int pb)
		{
			if (pb > Base.kNumPosStatesBitsMax)
				throw new InvalidParamException();
			uint numPosStates = (uint)1 << pb;
			m_LenDecoder.Create(numPosStates);
			m_RepLenDecoder.Create(numPosStates);
			m_PosStateMask = numPosStates - 1;
		}

		bool _solid = false;

		void Init(System.IO.Stream inStream, System.IO.Stream outStream)
		{
			m_RangeDecoder.Init(inStream);
			m_OutWindow.Init(outStream, _solid);

			uint i;
			for (i = 0; i < Base.kNumStates; i++)
			{
				for (uint j = 0; j <= m_PosStateMask; j++)
				{
					uint index = (i << Base.kNumPosStatesBitsMax) + j;
					m_IsMatchDecoders[index].Init();
					m_IsRep0LongDecoders[index].Init();
				}
				m_IsRepDecoders[i].Init();
				m_IsRepG0Decoders[i].Init();
				m_IsRepG1Decoders[i].Init();
				m_IsRepG2Decoders[i].Init();
			}

			m_LiteralDecoder.Init();
			for (i = 0; i < Base.kNumLenToPosStates; i++)
				m_PosSlotDecoder[i].Init();
			// m_PosSpecDecoder.Init();
			for (i = 0; i < Base.kNumFullDistances - Base.kEndPosModelIndex; i++)
				m_PosDecoders[i].Init();

			m_LenDecoder.Init();
			m_RepLenDecoder.Init();
			m_PosAlignDecoder.Init();
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
				if (m_IsMatchDecoders[state.Index << Base.kNumPosStatesBitsMax].Decode(m_RangeDecoder) != 0)
					throw new DataErrorException();
				state.UpdateChar();
				byte b = m_LiteralDecoder.DecodeNormal(m_RangeDecoder, 0, 0);
				m_OutWindow.PutByte(b);
				nowPos64++;
			}
			while (nowPos64 < outSize64)
			{
				// UInt64 next = Math.Min(nowPos64 + (1 << 18), outSize64);
					// while(nowPos64 < next)
				{
					uint posState = (uint)nowPos64 & m_PosStateMask;
					if (m_IsMatchDecoders[(state.Index << Base.kNumPosStatesBitsMax) + posState].Decode(m_RangeDecoder) == 0)
					{
						byte b;
						byte prevByte = m_OutWindow.GetByte(0);
						if (!state.IsCharState())
							b = m_LiteralDecoder.DecodeWithMatchByte(m_RangeDecoder,
								(uint)nowPos64, prevByte, m_OutWindow.GetByte(rep0));
						else
							b = m_LiteralDecoder.DecodeNormal(m_RangeDecoder, (uint)nowPos64, prevByte);
						m_OutWindow.PutByte(b);
						state.UpdateChar();
						nowPos64++;
					}
					else
					{
						uint len;
						if (m_IsRepDecoders[state.Index].Decode(m_RangeDecoder) == 1)
						{
							if (m_IsRepG0Decoders[state.Index].Decode(m_RangeDecoder) == 0)
							{
								if (m_IsRep0LongDecoders[(state.Index << Base.kNumPosStatesBitsMax) + posState].Decode(m_RangeDecoder) == 0)
								{
									state.UpdateShortRep();
									m_OutWindow.PutByte(m_OutWindow.GetByte(rep0));
									nowPos64++;
									continue;
								}
							}
							else
							{
								UInt32 distance;
								if (m_IsRepG1Decoders[state.Index].Decode(m_RangeDecoder) == 0)
								{
									distance = rep1;
								}
								else
								{
									if (m_IsRepG2Decoders[state.Index].Decode(m_RangeDecoder) == 0)
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
							len = m_RepLenDecoder.Decode(m_RangeDecoder, posState) + Base.kMatchMinLen;
							state.UpdateRep();
						}
						else
						{
							rep3 = rep2;
							rep2 = rep1;
							rep1 = rep0;
							len = Base.kMatchMinLen + m_LenDecoder.Decode(m_RangeDecoder, posState);
							state.UpdateMatch();
							uint posSlot = m_PosSlotDecoder[Base.GetLenToPosState(len)].Decode(m_RangeDecoder);
							if (posSlot >= Base.kStartPosModelIndex)
							{
								int numDirectBits = (int)((posSlot >> 1) - 1);
								rep0 = ((2 | (posSlot & 1)) << numDirectBits);
								if (posSlot < Base.kEndPosModelIndex)
									rep0 += BitTreeDecoder.ReverseDecode(m_PosDecoders,
											rep0 - posSlot - 1, m_RangeDecoder, numDirectBits);
								else
								{
									rep0 += (m_RangeDecoder.DecodeDirectBits(
										numDirectBits - Base.kNumAlignBits) << Base.kNumAlignBits);
									rep0 += m_PosAlignDecoder.ReverseDecode(m_RangeDecoder);
								}
							}
							else
								rep0 = posSlot;
						}
						if (rep0 >= m_OutWindow.TrainSize + nowPos64 || rep0 >= m_DictionarySizeCheck)
						{
							if (rep0 == 0xFFFFFFFF)
								break;
							throw new DataErrorException();
						}
						m_OutWindow.CopyBlock(rep0, len);
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
			return m_OutWindow.Train(stream);
		}
	}
}
