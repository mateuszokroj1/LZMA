using Lzma.Structs;

namespace Lzma.Coders
{
    internal class LengthEncoder
    {
        #region Constructor

        public LengthEncoder()
        {
            for (uint posState = 0; posState < Base.kNumPosStatesEncodingMax; ++posState)
            {
                this.lowCoder[posState] = new BitTreeEncoder(Base.kNumLowLenBits);
                this.midCoder[posState] = new BitTreeEncoder(Base.kNumMidLenBits);
            }
        }

        #endregion

        #region Fields

        private BitEncoder choice = new BitEncoder();
        private BitEncoder choice2 = new BitEncoder();
        private readonly BitTreeEncoder[] lowCoder = new BitTreeEncoder[Base.kNumPosStatesEncodingMax];
        private readonly BitTreeEncoder[] midCoder = new BitTreeEncoder[Base.kNumPosStatesEncodingMax];
        private readonly BitTreeEncoder highCoder = new BitTreeEncoder(Base.kNumHighLenBits);

        #endregion

        #region Methods

        public void Init(uint numPosStates)
        {
            this.choice.Init();
            this.choice2.Init();

            for (uint posState = 0; posState < numPosStates; ++posState)
            {
                this.lowCoder[posState].Init();
                this.midCoder[posState].Init();
            }

            this.highCoder.Init();
        }

        public void Encode(RangeEncoder rangeEncoder, uint symbol, uint posState)
        {
            if (symbol < Base.kNumLowLenSymbols)
            {
                this.choice.Encode(rangeEncoder, 0);
                this.lowCoder[posState].Encode(rangeEncoder, symbol);
            }
            else
            {
                symbol -= Base.kNumLowLenSymbols;
                this.choice.Encode(rangeEncoder, 1);

                if (symbol < Base.kNumMidLenSymbols)
                {
                    this.choice2.Encode(rangeEncoder, 0);
                    this.midCoder[posState].Encode(rangeEncoder, symbol);
                }
                else
                {
                    this.choice2.Encode(rangeEncoder, 1);
                    this.highCoder.Encode(rangeEncoder, symbol - Base.kNumMidLenSymbols);
                }
            }
        }

        public void SetPrices(uint posState, uint numSymbols, uint[] prices, uint st)
        {
            uint a0 = this.choice.Price0;
            uint a1 = this.choice.Price1;
            uint b0 = a1 + this.choice2.Price0;
            uint b1 = a1 + this.choice2.Price1;
            uint i = 0;

            for (i = 0; i < Base.kNumLowLenSymbols; ++i)
            {
                if (i >= numSymbols)
                    return;

                prices[st + i] = a0 + this.lowCoder[posState].GetPrice(i);
            }

            for (; i < Base.kNumLowLenSymbols + Base.kNumMidLenSymbols; ++i)
            {
                if (i >= numSymbols)
                    return;

                prices[st + i] = b0 + this.midCoder[posState].GetPrice(i - Base.kNumLowLenSymbols);
            }

            for (; i < numSymbols; ++i)
                prices[st + i] = b1 +
                    this.highCoder.GetPrice(i - Base.kNumLowLenSymbols - Base.kNumMidLenSymbols);
        }

        #endregion
    }
}
