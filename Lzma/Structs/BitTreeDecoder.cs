using Lzma.Coders;

namespace Lzma.Structs
{
    internal struct BitTreeDecoder
    {
        #region Constructor

        public BitTreeDecoder(int numBitLevels)
        {
            this.numBitLevels = numBitLevels;
<<<<<<< HEAD
            this.models = new BitDecoder[1 << numBitLevels];
=======
            models = new BitDecoder[1 << numBitLevels];
>>>>>>> 268951ff6b669f74e4c52cfe9fd98370eac7b51d
        }

        #endregion

        #region Fields

        private readonly BitDecoder[] models;
        private readonly int numBitLevels;

        #endregion

        #region Methods

        public void Init()
        {
<<<<<<< HEAD
            for (uint i = 1; i < 1 << this.numBitLevels; i++)
                this.models[i].Init();
        }

        public uint Decode(RangeDecoder rangeDecoder)
        {
            uint m = 1;

            for (int bitIndex = this.numBitLevels; bitIndex > 0; bitIndex--)
                m = (m << 1) + this.models[m].Decode(rangeDecoder);

            return m - ((uint)1 << this.numBitLevels);
        }

        public uint ReverseDecode(RangeDecoder rangeDecoder)
=======
            for (uint i = 1; i < 1 << numBitLevels; i++)
                models[i].Init();
        }

        public uint Decode(Coders.LzDecoder rangeDecoder)
        {
            uint m = 1;

            for (int bitIndex = numBitLevels; bitIndex > 0; bitIndex--)
                m = (m << 1) + models[m].Decode(rangeDecoder);

            return m - ((uint)1 << numBitLevels);
        }

        public uint ReverseDecode(Coders.LzDecoder rangeDecoder)
>>>>>>> 268951ff6b669f74e4c52cfe9fd98370eac7b51d
        {
            uint m = 1;
            uint symbol = 0;

<<<<<<< HEAD
            for (int bitIndex = 0; bitIndex < this.numBitLevels; bitIndex++)
            {
                uint bit = this.models[m].Decode(rangeDecoder);
=======
            for (int bitIndex = 0; bitIndex < numBitLevels; bitIndex++)
            {
                uint bit = models[m].Decode(rangeDecoder);
>>>>>>> 268951ff6b669f74e4c52cfe9fd98370eac7b51d
                m <<= 1;
                m += bit;
                symbol |= bit << bitIndex;
            }

            return symbol;
        }

        public static uint ReverseDecode(BitDecoder[] models, uint startIndex,
<<<<<<< HEAD
            RangeDecoder rangeDecoder, int numBitLevels)
=======
            Coders.LzDecoder rangeDecoder, int numBitLevels)
>>>>>>> 268951ff6b669f74e4c52cfe9fd98370eac7b51d
        {
            uint m = 1;
            uint symbol = 0;

            for (int bitIndex = 0; bitIndex < numBitLevels; bitIndex++)
            {
                uint bit = models[startIndex + m].Decode(rangeDecoder);
                m <<= 1;
                m += bit;
                symbol |= bit << bitIndex;
            }

            return symbol;
        }

        #endregion
    }
}
