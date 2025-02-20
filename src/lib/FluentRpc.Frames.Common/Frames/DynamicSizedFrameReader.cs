using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using FluentRpc.Common;
using FluentRpc.Connections.Abstraction;
using FluentRpc.Frames.Abstraction;
using Microsoft.Extensions.Options;

namespace FluentRpc.Frames;

public class DynamicSizedFrameReader : IFrameReader<DynamicFrame, IConnection, DynamicFrameReadOptions>
{
    public async Task<Result<DynamicFrame>> ReadAsync(
        DynamicFrameReadOptions frameReadOptions,
        CancellationToken cancellationToken = default
    )
    {
        throw new Exception();
    }

    private async Task<Result<DynamicFrame>> ReadWithoutPrefix(
        DynamicFrameReaderOptions readerOptions,
        DynamicFrameReadOptions readOptions
    )
    {
        var connection = readOptions.Connection;

        var packetResult = await connection.ReceiveAtLeastBytes(new ReceiveAtLeastBytesOptions
        {
            AtLeast = sizeof(int),
            RestBufferSize = readerOptions.BufferSize,
            Pool = readOptions.Pool,
        });

        if (!packetResult.IsOk(out var packet))
        {
            return packetResult.Fail<ReadPacket, DynamicFrame>();
        }

        var (sizeBuffer, size) = packet.ReleaseOwnership();

        using (sizeBuffer)
        {
            if (size < sizeof(int))
            {
                // RETURN ERROR
            }

            if (size == sizeof(int))
            {
                var sizeBytes = sizeBuffer.Memory[..size].Span;
                var incomingFrameSize = BitConverter.IsLittleEndian
                    ? BinaryPrimitives.ReadInt32LittleEndian(sizeBytes)
                    : BinaryPrimitives.ReadInt32BigEndian(sizeBytes);
            }
        }

        throw new Exception();
    }
}

public class DynamicFrameReadOptions : IFrameReadOptions<IConnection>
{
    public required IOptionsSnapshot<DynamicFrameReaderOptions> ReadOptions { get; init; }
    public required IConnection Connection { get; init; }
    public MemoryPool<byte>? Pool { get; init; }
}

public class DynamicFrameReaderOptions
{
    public string? Prefix { get; init; }
    public required int BufferSize { get; init; }
    public required Encoding Encoding { get; init; }
}