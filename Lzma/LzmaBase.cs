namespace Lzma
{
	internal abstract class Base
	{
		private const uint kNumRepDistances = 4;
		private const uint kNumStates = 12;
		private const int kNumPosSlotBits = 6;
		private const int kDicLogSizeMin = 0;
		private const int kNumLenToPosStatesBits = 2;
		private const uint kNumLenToPosStates = 1 << kNumLenToPosStatesBits;
		private const uint kMatchMinLen = 2;

		public static uint GetLenToPosState(uint len)
		{
			len -= kMatchMinLen;

            return len < kNumLenToPosStates ? len : kNumLenToPosStates - 1;
        }

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
	}
}
