using Lzma.Structs;

namespace Lzma.Coders
{
    internal class LiteralEncoder
    {
        #region Fields

        private Encoder2[] coders;
        private int numPreviousBits;
        private int numPositionBits;
        private uint positionMask;

        #endregion

        #region Methods

        public void Create(int numPosBits, int numPrevBits)
        {
            if (this.coders != null && this.numPreviousBits == numPrevBits && this.numPositionBits == numPosBits)
                return;

            this.numPositionBits = numPosBits;
            this.positionMask = ((uint)1 << numPosBits) - 1;
            this.numPreviousBits = numPrevBits;
            uint numStates = (uint)1 << (this.numPreviousBits + this.numPositionBits);
            this.coders = new Encoder2[numStates];

            for (uint i = 0; i < numStates; ++i)
                this.coders[i].Create();
        }

        public void Init()
        {
            uint numStates = (uint)1 << (this.numPreviousBits + this.numPositionBits);

            for (uint i = 0; i < numStates; ++i)
                this.coders[i].Init();
        }

        public Encoder2 GetSubCoder(uint position, byte previousByte) =>
            this.coders[((position & this.positionMask) << this.numPreviousBits) + (uint)(previousByte >> (8 - this.numPreviousBits))];

        #endregion
    }
}
