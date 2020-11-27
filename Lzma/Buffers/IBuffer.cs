using System.IO;

namespace Lzma.Buffers
{
    internal interface IBuffer
    {
        uint Length { get; }
        ulong ProcessedSize { get; }
        Stream Stream { get; set; }
    }
}