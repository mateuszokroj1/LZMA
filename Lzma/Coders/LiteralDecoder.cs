
using Lzma.Structs;

namespace Lzma.Coders
{
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
        }

        public void Init()
        {
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
    };
}
