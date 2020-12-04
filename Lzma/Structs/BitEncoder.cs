using Lzma.Coders;

namespace Lzma.Structs
{
    internal struct BitEncoder
    {
        #region Constructor

        static BitEncoder()
        {
            const int kNumBits = kNumBitModelTotalBits - kNumMoveReducingBits;

            for (int i = kNumBits - 1; i >= 0; i--)
            {
                uint start = (uint) 1 << (kNumBits - i - 1);
                uint end = (uint) 1 << (kNumBits - i);

                for (uint j = start; j < end; j++)
                    ProbPrices[j] = ((uint)i << kNumBitPriceShiftBits) +
                        (((end - j) << kNumBitPriceShiftBits) >> (kNumBits - i - 1));
            }
        }

        #endregion

        #region Fields

        public const int kNumBitModelTotalBits = 11;
        public const uint kBitModelTotal = 1 << kNumBitModelTotalBits;
        private const int kNumMoveBits = 5;
        private const int kNumMoveReducingBits = 2;
        public const int kNumBitPriceShiftBits = 6;

        private uint Prob;

        private static readonly uint[] ProbPrices = new uint[kBitModelTotal >> kNumMoveReducingBits];

        #endregion

        #region Properties

        public uint Price0 => ProbPrices[Prob >> kNumMoveReducingBits];
        public uint Price1 => ProbPrices[(kBitModelTotal - Prob) >> kNumMoveReducingBits];

        #endregion

        public void Init() => Prob = kBitModelTotal >> 1;

        public void UpdateModel(uint symbol)
        {
            if (symbol == 0)
                Prob += (kBitModelTotal - Prob) >> kNumMoveBits;
            else
                Prob -= (Prob) >> kNumMoveBits;
        }

<<<<<<< HEAD
        public void Encode(RangeEncoder encoder, uint symbol)
=======
        public void Encode(LzEncoder encoder, uint symbol)
>>>>>>> 268951ff6b669f74e4c52cfe9fd98370eac7b51d
        {
            uint newBound = (encoder.Range >> kNumBitModelTotalBits) * Prob;

            if (symbol == 0)
            {
                encoder.Range = newBound;
                Prob += (kBitModelTotal - Prob) >> kNumMoveBits;
            }
            else
            {
                encoder.Low += newBound;
                encoder.Range -= newBound;
                Prob -= (Prob) >> kNumMoveBits;
            }

<<<<<<< HEAD
            if (encoder.Range < RangeEncoder.kTopValue)
=======
            if (encoder.Range < LzEncoder.kTopValue)
>>>>>>> 268951ff6b669f74e4c52cfe9fd98370eac7b51d
            {
                encoder.Range <<= 8;
                encoder.ShiftLow();
            }
        }

        public uint GetPrice(uint symbol) =>
            ProbPrices[(((Prob - symbol) ^ (-(int)symbol)) & (kBitModelTotal - 1)) >> kNumMoveReducingBits];
    }
}
