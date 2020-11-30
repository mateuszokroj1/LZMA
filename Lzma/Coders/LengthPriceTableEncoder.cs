namespace Lzma.Coders
{
    internal class LengthPriceTableEncoder : LengthEncoder
    {
        #region Fields

        private readonly uint[] prices = new uint[Base.kNumLenSymbols << Base.kNumPosStatesBitsEncodingMax];
        private readonly uint[] counters = new uint[Base.kNumPosStatesEncodingMax];

        #endregion

        #region Properties

        public uint TableSize { get; set; }

        #endregion

        #region Methods

        public uint GetPrice(uint symbol, uint posState) =>
            this.prices[posState * Base.kNumLenSymbols + symbol];

        private void UpdateTable(uint positionState)
        {
            SetPrices(positionState, TableSize, prices, positionState * Base.kNumLenSymbols);
            counters[positionState] = TableSize;
        }

        public void UpdateTables(uint numPositionStates)
        {
            for (uint posState = 0; posState < numPositionStates; ++posState)
                UpdateTable(posState);
        }

        public new void Encode(RangeEncoder rangeEncoder, uint symbol, uint positionState)
        {
            base.Encode(rangeEncoder, symbol, positionState);

            if (--counters[positionState] == 0)
                UpdateTable(positionState);
        }

        #endregion
    }
}
