using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lzma.Structs
{
    struct Decoder2
    {
        BitDecoder[] m_Decoders;
        public void Create() { m_Decoders = new BitDecoder[0x300]; }
        public void Init() { for (int i = 0; i < 0x300; i++) m_Decoders[i].Init(); }

        public byte DecodeNormal(RangeCoder.Decoder rangeDecoder)
        {
            uint symbol = 1;
            do
                symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
            while (symbol < 0x100);
            return (byte)symbol;
        }

        public byte DecodeWithMatchByte(RangeCoder.Decoder rangeDecoder, byte matchByte)
        {
            uint symbol = 1;
            do
            {
                uint matchBit = (uint)(matchByte >> 7) & 1;
                matchByte <<= 1;
                uint bit = m_Decoders[((1 + matchBit) << 8) + symbol].Decode(rangeDecoder);
                symbol = (symbol << 1) | bit;
                if (matchBit != bit)
                {
                    while (symbol < 0x100)
                        symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
                    break;
                }
            }
            while (symbol < 0x100);
            return (byte)symbol;
        }
    }
}
