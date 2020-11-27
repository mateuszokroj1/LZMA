using System;

namespace Lzma
{
    [Serializable]
    public class InvalidDataException : ArgumentException
    {
        public InvalidDataException() : base("Invalid parameter.") { }
    }
}
