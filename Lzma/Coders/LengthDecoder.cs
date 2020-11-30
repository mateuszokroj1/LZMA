using Lzma.Structs;

namespace Lzma.Coders
{
    internal class LengthDecoder
    {
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
        }

        public void Init()
        {
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
    }
}
