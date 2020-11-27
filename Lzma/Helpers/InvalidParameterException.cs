using System;

namespace Lzma
{
    [Serializable]
    public class InvalidParameterException : ArgumentException
    {
        public InvalidParameterException() : base("Invalid parameter.") { }
    }
}
