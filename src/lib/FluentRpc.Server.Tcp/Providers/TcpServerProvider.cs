using FluentRpc.Common;
using FluentRpc.Providers.Abstraction;
using FluentRpc.Tcp.Connections;
using FluentRpc.Tcp.Server;

namespace FluentRpc.Tcp.Providers;

public class TcpServerProvider : IServerProvider<TcpConnection, TcpServer, TcpServerResult>
{
    public ValueTask<Result<TcpServerResult>> Create()
    {
        try
        {
            var res = new TcpServerResult { Server = new TcpServer() };
            return ValueTask.FromResult(Result<TcpServerResult>.Ok(res));
        }
        catch (Exception e)
        {
            return ValueTask.FromResult(Result<TcpServerResult>.Failure(e));
        }
    }
}

public class TcpServerResult : IServerResult<TcpServer, TcpConnection>
{
    public required TcpServer Server { get; init; }
}