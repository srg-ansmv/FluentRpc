using System.Net.Sockets;
using FluentRpc.Common;
using FluentRpc.Connections.Abstraction;

namespace FluentRpc.Tcp.Connections;

public class TcpConnection : IConnection, IDisposable
{
    private readonly NetworkStream _socket;

    public TcpConnection(Socket socket)
    {
        _socket = new NetworkStream(socket, ownsSocket: true);
    }

    public async Task<Result<ReadPacket>> ReceiveBytes(ReceiveBytesOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var owner = options.Pool?.Rent(options.BufferSize) ?? new NotPooledMemoryOwner(new byte[options.BufferSize]);
            var readCount = await _socket.ReadAsync(owner.Memory, cancellationToken);
            return Result<ReadPacket>.Ok(new(readCount, owner));
        }
        catch (Exception e)
        {
            return Result<ReadPacket>.Failure(e);
        }
    }

    public async Task<Result> WriteBytes(WritePacket packet, CancellationToken cancellationToken = default)
    {
        try
        {
            await _socket.WriteAsync(packet.Memory, cancellationToken);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Failure(e);
        }
    }

    public async Task<Result<ReadPacket>> ReceiveAtLeastBytes(
        ReceiveAtLeastBytesOptions options,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var bufferSize = options.AtLeast + options.RestBufferSize;
            var owner = options.Pool?.Rent(bufferSize) ?? new NotPooledMemoryOwner(new byte[bufferSize]);
            var readCount = await _socket.ReadAtLeastAsync(owner.Memory, options.AtLeast, true, cancellationToken);
            
            return Result<ReadPacket>.Ok(new(readCount, owner));
        }
        catch (Exception e)
        {
            return Result<ReadPacket>.Failure(e);
        }
    }

    public void Dispose()
    {
        _socket.Dispose();
    }
}