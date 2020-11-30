
using Lzma.Structs;

namespace Lzma.Coders
{
    internal class LiteralDecoder
    {
        #region Fields

        private Decoder2[] coders;
        private int numPreviousBits;
        private int numPositionBits;
        private uint positionMask;

        #endregion

        #region Methods

        public void Create(int numPositionBits, int numPreviousBits)
        {
            if (this.coders != null && this.numPreviousBits == numPreviousBits &&
                this.numPositionBits == numPositionBits)
                return;

            this.numPositionBits = numPositionBits;
            this.positionMask = ((uint)1 << this.numPositionBits) - 1;
            this.numPreviousBits = numPreviousBits;
            uint numStates = (uint)1 << (this.numPreviousBits + this.numPositionBits);
            this.coders = new Decoder2[numStates];

            for (uint i = 0; i < numStates; ++i)
                this.coders[i].Create();
        }

        public void Init()
        {
            uint numStates = (uint)1 << (this.numPreviousBits + this.numPositionBits);

            for (uint i = 0; i < numStates; ++i)
                this.coders[i].Init();
        }

        private uint GetState(uint position, byte previousByte) =>
            ((position & this.positionMask) << this.numPreviousBits) + (uint)(previousByte >> (8 - this.numPreviousBits));

        public byte DecodeNormal(RangeDecoder rangeDecoder, uint position, byte previousByte) =>
            this.coders[GetState(position, previousByte)].DecodeNormal(rangeDecoder);

        public byte DecodeWithMatchByte(RangeDecoder rangeDecoder, uint position, byte previousByte, byte matchByte) =>
            this.coders[GetState(position, previousByte)].DecodeWithMatchByte(rangeDecoder, matchByte);

        #endregion
    };
}
