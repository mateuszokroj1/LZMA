using Lzma.Structs;

namespace Lzma.Coders
{
    internal class LengthDecoder
    {
        BitDecoder m_Choice = new BitDecoder();
        BitDecoder m_Choice2 = new BitDecoder();
        BitTreeDecoder[] m_LowCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
        BitTreeDecoder[] m_MidCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
        BitTreeDecoder m_HighCoder = new BitTreeDecoder(Base.kNumHighLenBits);
        uint m_NumPosStates = 0;

        public void Create(uint numPosStates)
        {
            for (uint posState = m_NumPosStates; posState < numPosStates; posState++)
            {
                m_LowCoder[posState] = new BitTreeDecoder(Base.kNumLowLenBits);
                m_MidCoder[posState] = new BitTreeDecoder(Base.kNumMidLenBits);
            }
            m_NumPosStates = numPosStates;
        }

        public void Init()
        {
            m_Choice.Init();
            for (uint posState = 0; posState < m_NumPosStates; posState++)
            {
                m_LowCoder[posState].Init();
                m_MidCoder[posState].Init();
            }
            m_Choice2.Init();
            m_HighCoder.Init();
        }

        public uint Decode(Decoder rangeDecoder, uint posState)
        {
            if (m_Choice.Decode(rangeDecoder) == 0)
                return m_LowCoder[posState].Decode(rangeDecoder);
            else
            {
                uint symbol = Base.kNumLowLenSymbols;
                if (m_Choice2.Decode(rangeDecoder) == 0)
                    symbol += m_MidCoder[posState].Decode(rangeDecoder);
                else
                {
                    symbol += Base.kNumMidLenSymbols;
                    symbol += m_HighCoder.Decode(rangeDecoder);
                }
                return symbol;
            }
        }
    }
}
