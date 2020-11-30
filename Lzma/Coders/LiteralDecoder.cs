
using Lzma.Structs;

namespace Lzma.Coders
{
    internal class LiteralDecoder
    {
        #region Fields

        Decoder2[] coders;
        int numPrevBits;
        int numPosBits;
        uint positionMask;

        #endregion

        public void Create(int numPosBits, int numPrevBits)
        {
            if (this.coders != null && this.numPrevBits == numPrevBits &&
                this.numPosBits == numPosBits)
                return;

            this.numPosBits = numPosBits;
            positionMask = ((uint)1 << numPosBits) - 1;
            this.numPrevBits = numPrevBits;
            uint numStates = (uint)1 << (this.numPrevBits + this.numPosBits);
            coders = new Decoder2[numStates];
            for (uint i = 0; i < numStates; i++)
                coders[i].Create();
        }

        public void Init()
        {
            uint numStates = (uint)1 << (numPrevBits + numPosBits);
            for (uint i = 0; i < numStates; i++)
                coders[i].Init();
        }

        uint GetState(uint pos, byte prevByte)
        { return ((pos & positionMask) << numPrevBits) + (uint)(prevByte >> (8 - numPrevBits)); }

        public byte DecodeNormal(RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte)
        { return coders[GetState(pos, prevByte)].DecodeNormal(rangeDecoder); }

        public byte DecodeWithMatchByte(RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte, byte matchByte)
        { return coders[GetState(pos, prevByte)].DecodeWithMatchByte(rangeDecoder, matchByte); }
    };
}
