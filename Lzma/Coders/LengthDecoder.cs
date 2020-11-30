using Lzma.Structs;

namespace Lzma.Coders
{
    internal class LengthDecoder
    {
        #region Fields

        private BitDecoder choice = new BitDecoder();
        private BitDecoder choice2 = new BitDecoder();
        private BitTreeDecoder[] lowCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
        private BitTreeDecoder[] midCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
        private BitTreeDecoder highCoder = new BitTreeDecoder(Base.kNumHighLenBits);
        private uint numPosStates = 0;

        #endregion

        #region Methods

        public void Create(uint numPosStates)
        {
            for (uint posState = this.numPosStates; posState < numPosStates; posState++)
            {
                this.lowCoder[posState] = new BitTreeDecoder(Base.kNumLowLenBits);
                this.midCoder[posState] = new BitTreeDecoder(Base.kNumMidLenBits);
            }

            this.numPosStates = numPosStates;
        }

        public void Init()
        {
            choice.Init();
            for (uint posState = 0; posState < numPosStates; posState++)
            {
                lowCoder[posState].Init();
                midCoder[posState].Init();
            }
            choice2.Init();
            highCoder.Init();
        }

        public uint Decode(RangeDecoder rangeDecoder, uint posState)
        {
            if (choice.Decode(rangeDecoder) == 0)
                return lowCoder[posState].Decode(rangeDecoder);
            else
            {
                uint symbol = Base.kNumLowLenSymbols;
                if (choice2.Decode(rangeDecoder) == 0)
                    symbol += midCoder[posState].Decode(rangeDecoder);
                else
                {
                    symbol += Base.kNumMidLenSymbols;
                    symbol += highCoder.Decode(rangeDecoder);
                }
                return symbol;
            }
        }

        #endregion
    }
}
