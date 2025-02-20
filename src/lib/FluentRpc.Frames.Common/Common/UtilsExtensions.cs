using System.Text;

namespace FluentRpc.Common;

internal static class UtilsExtensions
{
    public static ReadOnlyMemory<byte> Encode(this string src, Encoding encoding)
        => encoding.GetBytes(src);
}