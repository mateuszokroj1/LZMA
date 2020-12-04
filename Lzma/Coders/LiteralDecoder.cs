
using Lzma.Structs;

namespace Lzma.Coders
{
<<<<<<< HEAD
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
=======
    class LiteralDecoder
    {
        

        Decoder2[] m_Coders;
        int m_NumPrevBits;
        int m_NumPosBits;
        uint m_PosMask;

        public void Create(int numPosBits, int numPrevBits)
        {
            if (m_Coders != null && m_NumPrevBits == numPrevBits &&
                m_NumPosBits == numPosBits)
                return;
            m_NumPosBits = numPosBits;
            m_PosMask = ((uint)1 << numPosBits) - 1;
            m_NumPrevBits = numPrevBits;
            uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);
            m_Coders = new Decoder2[numStates];
            for (uint i = 0; i < numStates; i++)
                m_Coders[i].Create();
>>>>>>> 268951ff6b669f74e4c52cfe9fd98370eac7b51d
        }

        public void Init()
        {
<<<<<<< HEAD
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
=======
            uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);
            for (uint i = 0; i < numStates; i++)
                m_Coders[i].Init();
        }

        uint GetState(uint pos, byte prevByte)
        { return ((pos & m_PosMask) << m_NumPrevBits) + (uint)(prevByte >> (8 - m_NumPrevBits)); }

        public byte DecodeNormal(RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte)
        { return m_Coders[GetState(pos, prevByte)].DecodeNormal(rangeDecoder); }

        public byte DecodeWithMatchByte(RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte, byte matchByte)
        { return m_Coders[GetState(pos, prevByte)].DecodeWithMatchByte(rangeDecoder, matchByte); }
>>>>>>> 268951ff6b669f74e4c52cfe9fd98370eac7b51d
    };
}
