using FluentRpc.Common;
using FluentRpc.Connections.Abstraction;
using FluentRpc.Server.Abstraction;

namespace FluentRpc.Providers.Abstraction;

public interface IServerProvider<TConnection, TServer, TServerResult>
    where TServerResult : IServerResult<TServer, TConnection>
    where TServer : IServer<TConnection>
    where TConnection : IConnection
{
    ValueTask<Result<TServerResult>> Create();
}

public interface IServerResult<out TServer, TConnection>
    where TServer : IServer<TConnection>
    where TConnection : IConnection
{
    public TServer Server { get; }
}