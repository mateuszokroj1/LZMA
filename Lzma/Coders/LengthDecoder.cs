using Lzma.Structs;

namespace Lzma.Coders
{
    internal class LengthDecoder
    {
<<<<<<< HEAD
        #region Fields

        private BitDecoder choice = new BitDecoder();
        private BitDecoder choice2 = new BitDecoder();
        private readonly BitTreeDecoder[] lowCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
        private readonly BitTreeDecoder[] midCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
        private readonly BitTreeDecoder highCoder = new BitTreeDecoder(Base.kNumHighLenBits);
        private uint numPosStates = 0;

        #endregion

        #region Methods

        public void Create(uint numPosStates)
        {
            for (uint posState = this.numPosStates; posState < numPosStates; ++posState)
            {
                this.lowCoder[posState] = new BitTreeDecoder(Base.kNumLowLenBits);
                this.midCoder[posState] = new BitTreeDecoder(Base.kNumMidLenBits);
            }

            this.numPosStates = numPosStates;
=======
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
>>>>>>> 268951ff6b669f74e4c52cfe9fd98370eac7b51d
        }

        public void Init()
        {
<<<<<<< HEAD
            this.choice.Init();
            for (uint posState = 0; posState < this.numPosStates; ++posState)
            {
                this.lowCoder[posState].Init();
                this.midCoder[posState].Init();
            }

            this.choice2.Init();
            this.highCoder.Init();
        }

        public uint Decode(RangeDecoder rangeDecoder, uint posState)
        {
            if (this.choice.Decode(rangeDecoder) == 0)
                return this.lowCoder[posState].Decode(rangeDecoder);
            else
            {
                uint symbol = Base.kNumLowLenSymbols;
                if (this.choice2.Decode(rangeDecoder) == 0)
                    symbol += this.midCoder[posState].Decode(rangeDecoder);
                else
                {
                    symbol += Base.kNumMidLenSymbols;
                    symbol += this.highCoder.Decode(rangeDecoder);
                }

                return symbol;
            }
        }

        #endregion
=======
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
>>>>>>> 268951ff6b669f74e4c52cfe9fd98370eac7b51d
    }
}
