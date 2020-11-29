using Lzma.Coders;

namespace Lzma.Structs
{
    internal struct BitTreeDecoder
    {
        #region Constructor

        public BitTreeDecoder(int numBitLevels)
        {
            this.numBitLevels = numBitLevels;
            models = new BitDecoder[1 << numBitLevels];
        }

        #endregion

        #region Fields

        private readonly BitDecoder[] models;
        private readonly int numBitLevels;

        #endregion

        #region Methods

        public void Init()
        {
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
        {
            uint m = 1;
            uint symbol = 0;

            for (int bitIndex = 0; bitIndex < numBitLevels; bitIndex++)
            {
                uint bit = models[m].Decode(rangeDecoder);
                m <<= 1;
                m += bit;
                symbol |= bit << bitIndex;
            }

            return symbol;
        }

        public static uint ReverseDecode(BitDecoder[] models, uint startIndex,
            Coders.LzDecoder rangeDecoder, int numBitLevels)
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
