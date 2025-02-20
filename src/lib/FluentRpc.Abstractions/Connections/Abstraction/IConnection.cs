using System.Buffers;
using FluentRpc.Common;

namespace FluentRpc.Connections.Abstraction;

public interface IConnection
{
    Task<Result<ReadPacket>> ReceiveBytes(ReceiveBytesOptions options, CancellationToken cancellationToken = default);

    Task<UnitResult> WriteBytes(WritePacket packet, CancellationToken cancellationToken = default);

    Task<Result<ReadPacket>> ReceiveAtLeastBytes(
        ReceiveAtLeastBytesOptions options,
        CancellationToken cancellationToken = default
    );
}

public readonly struct ReceiveAtLeastBytesOptions
{
    public required int AtLeast { get; init; }
    public required int RestBufferSize { get; init; }
    public MemoryPool<byte>? Pool { get; init; }
}

public readonly struct ReceiveBytesOptions
{
    public required int BufferSize { get; init; }
    public MemoryPool<byte>? Pool { get; init; }
}

public readonly struct WritePacket
{
    public required Memory<byte> Memory { get; init; }
}

public struct ReadPacket : IDisposable
{
    private IMemoryOwner<byte>? _memoryOwner;

    private bool _ownsBuffer = true;

    public ReadPacket(int readCount, IMemoryOwner<byte> memoryOwner)
    {
        ReadCount = readCount;
        _memoryOwner = memoryOwner;
    }

    public int ReadCount { get; }

    public readonly bool EndsWith(ReadOnlySpan<byte> bytes)
    {
        Protect();
        return _memoryOwner!.Memory.Span[..ReadCount].EndsWith(bytes);
    }

    public readonly bool StartsWith(ReadOnlySpan<byte> bytes)
    {
        Protect();
        return _memoryOwner!.Memory.Span[..ReadCount].StartsWith(bytes);
    }

    public readonly ReadOnlySpan<byte> Span()
    {
        Protect();
        return _memoryOwner!.Memory.Span;
    }

    public (IMemoryOwner<byte> Buffer, int Size) ReleaseOwnership()
    {
        Protect();

        _ownsBuffer = false;
        var @ref = _memoryOwner!;
        _memoryOwner = null;

        return (@ref, ReadCount);
    }

    public void Dispose()
    {
        Protect();
        _memoryOwner!.Dispose();
    }

    private readonly void Protect()
    {
        if (!_ownsBuffer || _memoryOwner is null)
        {
            throw new InvalidOperationException("Tried to use packet, but IMemoryOwner has been taken");
        }
    }
}