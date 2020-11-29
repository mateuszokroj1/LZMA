using System.IO;

namespace Lzma.Coders
{
    internal class LzEncoder
    {
        #region Fields

        public const uint kTopValue = 1 << 24;

        private uint cacheSize;
        private byte cache;

        #endregion

        #region Properties

        public Stream Stream { get; set; }

        public ulong Low { get; set; }

        public uint Range { get; set; }

        public long StartPosition { get; set; }

        public long ProcessedSize => this.cacheSize + Stream.Position - StartPosition + 4;

        #endregion

        public void Init()
        {
            StartPosition = Stream.Position;

            Low = 0;
            Range = 0xFFFFFFFF;
            this.cacheSize = 1;
            this.cache = 0;
        }

        public void FlushData()
        {
            for (int i = 0; i < 5; i++)
                ShiftLow();
        }

        public void Encode(uint start, uint size, uint total)
        {
            Low += start * (Range /= total);
            Range *= size;

            while (Range < kTopValue)
            {
                Range <<= 8;
                ShiftLow();
            }
        }

        public void ShiftLow()
        {
            if ((uint)Low < (uint)0xFF000000 || (uint)(Low >> 32) == 1)
            {
                byte temp = this.cache;

                do
                {
                    Stream.WriteByte((byte)(temp + (Low >> 32)));
                    temp = 0xFF;
                }
                while (--this.cacheSize != 0);

                this.cache = (byte)(((uint)Low) >> 24);
            }

            this.cacheSize++;
            Low = ((uint)Low) << 8;
        }

        public void EncodeDirectBits(uint v, int numTotalBits)
        {
            for (int i = numTotalBits - 1; i >= 0; i--)
            {
                Range >>= 1;

                if (((v >> i) & 1) == 1)
                    Low += Range;

                if (Range < kTopValue)
                {
                    Range <<= 8;
                    ShiftLow();
                }
            }
        }

        public void EncodeBit(uint size0, int numTotalBits, uint symbol)
        {
            uint newBound = (Range >> numTotalBits) * size0;

            if (symbol == 0)
                Range = newBound;
            else
            {
                Low += newBound;
                Range -= newBound;
            }

            while (Range < kTopValue)
            {
                Range <<= 8;
                ShiftLow();
            }
        }
    }
}
