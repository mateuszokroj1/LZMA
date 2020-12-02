using Lzma.Coders;

namespace Lzma.Structs
{
    internal struct BitTreeEncoder
    {
        #region Constructor

        public BitTreeEncoder(int numBitLevels)
        {
            this.numBitLevels = numBitLevels;
            this.models = new BitEncoder[1 << numBitLevels];
        }

        #endregion

        #region Fields

        private readonly BitEncoder[] models;
        private readonly int numBitLevels;

        #endregion

        #region Methods

        public void Init()
        {
            for (uint i = 1; i < 1 << numBitLevels; i++)
                models[i].Init();
        }

        public void Encode(RangeEncoder rangeEncoder, uint symbol)
        {
            uint m = 1;

            for (int bitIndex = numBitLevels; bitIndex > 0;)
            {
                bitIndex--;
                uint bit = (symbol >> bitIndex) & 1;
                models[m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
            }
        }

        public void ReverseEncode(RangeEncoder rangeEncoder, uint symbol)
        {
            uint m = 1;

            for (uint i = 0; i < numBitLevels; i++)
            {
                uint bit = symbol & 1;
                models[m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
                symbol >>= 1;
            }
        }

        public uint GetPrice(uint symbol)
        {
            uint price = 0;
            uint m = 1;

            for (int bitIndex = numBitLevels; bitIndex > 0;)
            {
                bitIndex--;
                uint bit = (symbol >> bitIndex) & 1;
                price += models[m].GetPrice(bit);
                m = (m << 1) + bit;
            }

            return price;
        }

        public uint ReverseGetPrice(uint symbol)
        {
            uint price = 0;
            uint m = 1;

            for (int i = numBitLevels; i > 0; i--)
            {
                uint bit = symbol & 1;
                symbol >>= 1;
                price += models[m].GetPrice(bit);
                m = (m << 1) | bit;
            }

            return price;
        }

        public static uint ReverseGetPrice(BitEncoder[] models, uint startIndex,
            int numBitLevels, uint symbol)
        {
            uint price = 0;
            uint m = 1;

            for (int i = numBitLevels; i > 0; i--)
            {
                uint bit = symbol & 1;
                symbol >>= 1;
                price += models[startIndex + m].GetPrice(bit);
                m = (m << 1) | bit;
            }

            return price;
        }

        public static void ReverseEncode(BitEncoder[] models, uint startIndex,
            RangeEncoder rangeEncoder, int numBitLevels, uint symbol)
        {
            uint m = 1;

            for (int i = 0; i < numBitLevels; i++)
            {
                uint bit = symbol & 1;
                models[startIndex + m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
                symbol >>= 1;
            }
        }

        #endregion
    }
}
