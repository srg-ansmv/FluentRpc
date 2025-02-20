using System.Buffers;

namespace FluentRpc.Common;

public class NotPooledMemoryOwner : IMemoryOwner<byte>
{
    public NotPooledMemoryOwner(byte[] memory)
    {
        Memory = memory;
    }

    public Memory<byte> Memory { get; }

    public void Dispose()
    {
    }
}