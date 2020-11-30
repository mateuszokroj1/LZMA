using Lzma.Coders;

namespace Lzma.Structs
{
    internal struct Encoder2
    {
        BitEncoder[] Encoders;

        private const int length = 0x300;

        public void Create() => Encoders = new BitEncoder[length];

        public void Init()
        {
            for (int i = 0; i < length; ++i)
                Encoders[i].Init();
        }

        public void Encode(RangeEncoder rangeEncoder, byte symbol)
        {
            uint context = 1;

            for (int i = 7; i >= 0; --i)
            {
                uint bit = (uint)((symbol >> i) & 1);
                Encoders[context].Encode(rangeEncoder, bit);
                context = (context << 1) | bit;
            }
        }

        public void EncodeMatched(RangeEncoder rangeEncoder, byte matchByte, byte symbol)
        {
            uint context = 1;
            bool same = true;
            for (int i = 7; i >= 0; --i)
            {
                uint bit = (uint)((symbol >> i) & 1);
                uint state = context;

                if (same)
                {
                    uint matchBit = (uint)((matchByte >> i) & 1);
                    state += (1 + matchBit) << 8;
                    same = matchBit == bit;
                }

                Encoders[state].Encode(rangeEncoder, bit);
                context = (context << 1) | bit;
            }
        }

        public uint GetPrice(bool matchMode, byte matchByte, byte symbol)
        {
            uint price = 0;
            uint context = 1;
            int i = 7;

            if (matchMode)
            {
                for (; i >= 0; --i)
                {
                    uint matchBit = (uint)(matchByte >> i) & 1;
                    uint bit = (uint)(symbol >> i) & 1;
                    price += Encoders[((1 + matchBit) << 8) + context].GetPrice(bit);
                    context = (context << 1) | bit;

                    if (matchBit != bit)
                    {
                        --i;
                        break;
                    }
                }
            }

            for (; i >= 0; --i)
            {
                uint bit = (uint)(symbol >> i) & 1;
                price += Encoders[context].GetPrice(bit);
                context = (context << 1) | bit;
            }

            return price;
        }
    }
}
