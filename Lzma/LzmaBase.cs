namespace Lzma
{
	public abstract class Base
	{
		protected const uint kNumRepDistances = 4;
		protected const uint kNumStates = 12;
		protected const int kNumPosSlotBits = 6;
		protected const int kDicLogSizeMin = 0;
		protected const int kNumLenToPosStatesBits = 2;
		protected const uint kNumLenToPosStates = 1 << kNumLenToPosStatesBits;
		protected const uint kMatchMinLen = 2;

        public const int kNumAlignBits = 4;
		public const uint kAlignTableSize = 1 << kNumAlignBits;
		public const uint kAlignMask = kAlignTableSize - 1;
		public const uint kStartPosModelIndex = 4;
		public const uint kEndPosModelIndex = 14;
		public const uint kNumPosModels = kEndPosModelIndex - kStartPosModelIndex;
		public const uint kNumFullDistances = 1 << ((int)kEndPosModelIndex / 2);
		public const uint kNumLitPosStatesBitsEncodingMax = 4;
		public const uint kNumLitContextBitsMax = 8;
		public const int kNumPosStatesBitsMax = 4;
		public const uint kNumPosStatesMax = 1 << kNumPosStatesBitsMax;
		public const int kNumPosStatesBitsEncodingMax = 4;
		public const uint kNumPosStatesEncodingMax = 1 << kNumPosStatesBitsEncodingMax;
		public const int kNumLowLenBits = 3;
		public const int kNumMidLenBits = 3;
		public const int kNumHighLenBits = 8;
		public const uint kNumLowLenSymbols = 1 << kNumLowLenBits;
		public const uint kNumMidLenSymbols = 1 << kNumMidLenBits;
		public const uint kNumLenSymbols = kNumLowLenSymbols + kNumMidLenSymbols +
				(1 << kNumHighLenBits);
		public const uint kMatchMaxLen = kMatchMinLen + kNumLenSymbols - 1;

        public static uint GetLenToPosState(uint length)
        {
            length -= kMatchMinLen;

            return length < kNumLenToPosStates ? length : kNumLenToPosStates - 1;
        }
    }
}
