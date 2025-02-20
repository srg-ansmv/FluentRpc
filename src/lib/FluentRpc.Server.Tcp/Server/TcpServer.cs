using System.Net;
using System.Net.Sockets;
using FluentRpc.Common;
using FluentRpc.Server.Abstraction;
using FluentRpc.Tcp.Connections;

namespace FluentRpc.Tcp.Server;

public class TcpServer : IServer<TcpConnection>
{
    private readonly Socket _socket;

    public TcpServer()
    {
        _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
    }

    public ValueTask<UnitResult> Listen(EndPoint endPoint, CancellationToken cancellationToken = default)
    {
        try
        {
            _socket.Bind(endPoint);
            _socket.Listen();
            return ValueTask.FromResult(UnitResult.Ok);
        }
        catch (Exception e)
        {
            return ValueTask.FromResult(UnitResult.Failure(e));
        }
    }

    public async Task<Result<TcpConnection>> AcceptClient(CancellationToken cancellationToken = default)
    {
        try
        {
            var socket = await _socket.AcceptAsync(cancellationToken);
            var connection = new TcpConnection(socket);
            return Result<TcpConnection>.Ok(connection);
        }
        catch (Exception e)
        {
            return Result<TcpConnection>.Failure(e);
        }
    }
}