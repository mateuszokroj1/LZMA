using Lzma.Coders;

namespace Lzma.Structs
{
    internal struct BitDecoder
    {
        #region Fields

        public const int kNumBitModelTotalBits = 11;
        public const uint kBitModelTotal = 1 << kNumBitModelTotalBits;
        private const int kNumMoveBits = 5;
        private const uint initProb = kBitModelTotal >> 1;
        uint Prob;

        #endregion

        #region Methods

        public void UpdateModel(int numMoveBits, uint symbol)
        {
            if (symbol == 0)
                Prob += (kBitModelTotal - Prob) >> numMoveBits;
            else
                Prob -= (Prob) >> numMoveBits;
        }

        public void Init() => Prob = initProb;

<<<<<<< HEAD
        public uint Decode(RangeDecoder rangeDecoder)
=======
        public uint Decode(Coders.LzDecoder rangeDecoder)
>>>>>>> 268951ff6b669f74e4c52cfe9fd98370eac7b51d
        {
            uint newBound = (rangeDecoder.Range >> kNumBitModelTotalBits) * Prob;
            if (rangeDecoder.Code < newBound)
            {
                rangeDecoder.Range = newBound;
                Prob += (kBitModelTotal - Prob) >> kNumMoveBits;

<<<<<<< HEAD
                if (rangeDecoder.Range < RangeDecoder.kTopValue)
=======
                if (rangeDecoder.Range < Coders.LzDecoder.kTopValue)
>>>>>>> 268951ff6b669f74e4c52cfe9fd98370eac7b51d
                {
                    rangeDecoder.Code = (rangeDecoder.Code << 8) | (byte)rangeDecoder.Stream.ReadByte();
                    rangeDecoder.Range <<= 8;
                }

                return 0;
            }
            else
            {
                rangeDecoder.Range -= newBound;
                rangeDecoder.Code -= newBound;
                Prob -= (Prob) >> kNumMoveBits;

<<<<<<< HEAD
                if (rangeDecoder.Range < RangeDecoder.kTopValue)
=======
                if (rangeDecoder.Range < Coders.LzDecoder.kTopValue)
>>>>>>> 268951ff6b669f74e4c52cfe9fd98370eac7b51d
                {
                    rangeDecoder.Code = (rangeDecoder.Code << 8) | (byte)rangeDecoder.Stream.ReadByte();
                    rangeDecoder.Range <<= 8;
                }

                return 1;
            }
        }

        #endregion
    }
}
