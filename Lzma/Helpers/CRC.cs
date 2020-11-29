namespace Lzma.Helpers
{
	internal class Crc
	{
        #region Constructor

        static Crc()
		{
			const uint kPoly = 0xEDB88320;

			for (uint i = 0; i < 256; i++)
			{
				uint r = i;

				for (int j = 0; j < 8; j++)
					if ((r & 1) != 0)
						r = (r >> 1) ^ kPoly;
					else
						r >>= 1;
                
				Table[i] = r;
			}
		}

        private Crc() { }

        #endregion

        #region Fields

        private uint value = initValue;

        private const uint initValue = 0xFFFFFFFF;

        #endregion

        #region Properties

        public static uint[] Table { get; } = new uint[256];

        public uint Digest => this.value ^ initValue;

        #endregion

        #region Methods

        public void UpdateByte(byte b) =>
            this.value = Table[((byte)this.value) ^ b] ^ (this.value >> 8);

        public void Update(byte[] data, uint offset, uint size)
		{
			for (uint i = 0; i < size; ++i)
				this.value = Table[((byte)this.value) ^ data[offset + i]] ^ (this.value >> 8);
		}

		public static uint CalculateDigest(byte[] data, uint offset, uint size)
		{
			var crc = new Crc();
			crc.Update(data, offset, size);
			return crc.Digest;
		}

        public static bool VerifyDigest(uint digest, byte[] data, uint offset, uint size) =>
            CalculateDigest(data, offset, size) == digest;

        #endregion
    }
}
