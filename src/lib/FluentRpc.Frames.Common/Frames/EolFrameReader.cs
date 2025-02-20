using System.Buffers;
using System.Text;
using FluentRpc.Common;
using FluentRpc.Connections.Abstraction;
using FluentRpc.Frames.Abstraction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentRpc.Frames;

public class EolFrameReader : IFrameReader<EolFrame, IConnection, EolFrameReadOptions>
{
    private readonly ILogger<EolFrameReader>? _logger;
    private readonly MemoryQueue _unusedMemory;

    public EolFrameReader(ILogger<EolFrameReader>? logger)
    {
        _logger = logger;
        _unusedMemory = new();
    }

    public async Task<Result<EolFrame>> ReadAsync(
        EolFrameReadOptions args,
        CancellationToken cancellationToken = default
    )
    {
        var options = args.ReadOptions.Value;
        var connection = args.Connection;

        _logger?.ReadStarted(connection);

        var eol = options.EndOfLine.Encode(options.Encoding);
        while (true)
        {
            var packetResult = await connection.ReceiveBytes(
                new() { BufferSize = options.BufferSize, Pool = args.Pool },
                cancellationToken
            );

            if (!packetResult.IsOk(out var readPacket))
            {
                return packetResult.Fail<ReadPacket, EolFrame>();
            }

            var eolSpan = eol.Span;
            if (readPacket.EndsWith(eolSpan))
            {
                return Result<EolFrame>.Ok(ProcessWhenPackageIsCompleted(readPacket, eolSpan, args.Pool));
            }

            var frameResult = ProcessWhenPackageIsNotCompleted(readPacket, eolSpan);
            if (frameResult.IsError(out var error))
            {
                _logger?.Error(connection, error.Value);
                continue;
            }

            return frameResult;
        }
    }

    private EolFrame ProcessWhenPackageIsCompleted(ReadPacket packet, ReadOnlySpan<byte> eol, MemoryPool<byte>? pool)
    {
        var split = packet.Span().Split(eol);

        var list = new List<Range>();
        while (split.MoveNext())
        {
            list.Add(split.Current);
        }

        var (buffer, size) = packet.ReleaseOwnership();
        _logger?.CompleteDataReceived(size);
        list.RemoveAt(list.Count - 1);

        return new EolFrame(buffer, size, list, _unusedMemory.DequeueAll(new() { MemoryPool = pool }));
    }

    private Result<EolFrame> ProcessWhenPackageIsNotCompleted(ReadPacket packet, ReadOnlySpan<byte> eol)
    {
        var split = packet.Span().Split(eol);

        var list = new List<Range>();
        while (split.MoveNext())
        {
            list.Add(split.Current);
        }

        if (list.Count == 1)
        {
            _unusedMemory.EnqueuePacket(packet);
            return Result<EolFrame>.Error(new("InternalReadError", "No separator where. cached out put"));
        }

        var (buffer, size) = packet.ReleaseOwnership();

        var lastItem = list[^1];
        _unusedMemory.EnqueueMemory(buffer.Memory[lastItem]);
        list.RemoveAt(list.Count - 1);
        _logger?.IncompleteDataReceived(size);

        return Result<EolFrame>.Ok(new EolFrame(buffer, size, list));
    }
}

public class EolFrameReadOptions : IFrameReadOptions<IConnection>
{
    public required IOptionsSnapshot<EolFrameReaderOptions> ReadOptions { get; init; }
    public required IConnection Connection { get; init; }
    public MemoryPool<byte>? Pool { get; init; }
}

public class EolFrameReaderOptions
{
    public required string EndOfLine { get; init; }
    public required int BufferSize { get; init; }
    public required Encoding Encoding { get; init; }
}

internal static partial class EolFrameReaderOptionsLogMsg
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "{Connection}: Error occured: {Error}")]
    public static partial void Error(this ILogger logger, IConnection connection, Error error);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Start reading from connection {Connection}")]
    public static partial void ReadStarted(this ILogger logger, IConnection connection);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Received {ReceivedBytes} bytes with complete frame")]
    public static partial void CompleteDataReceived(this ILogger logger, int receivedBytes);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = "Received {ReceivedBytes} bytes with incomplete frame (Some data stored)"
    )]
    public static partial void IncompleteDataReceived(this ILogger logger, int receivedBytes);
}