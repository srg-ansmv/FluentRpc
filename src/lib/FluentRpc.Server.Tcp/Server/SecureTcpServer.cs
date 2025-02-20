using System.Net;
using System.Net.Sockets;
using FluentRpc.Certificates.Abstractions;
using FluentRpc.Common;
using FluentRpc.Server.Abstraction;
using FluentRpc.Tcp.Connections;

namespace FluentRpc.Tcp.Server;

public class SecureTcpServer: IServer<ServerSslTcpConnection>
{
    private readonly ICertificateProvider _certificateProvider;
    private readonly Socket _socket;

    public SecureTcpServer(ICertificateProvider certificateProvider)
    {
        _certificateProvider = certificateProvider;
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

    public async Task<Result<ServerSslTcpConnection>> AcceptClient(CancellationToken cancellationToken = default)
    {
        try
        {
            var socket = await _socket.AcceptAsync(cancellationToken);
            var connection = new ServerSslTcpConnection(socket, _certificateProvider);
            return Result<ServerSslTcpConnection>.Ok(connection);
        }
        catch (Exception e)
        {
            return Result<ServerSslTcpConnection>.Failure(e);
        }
    }
}