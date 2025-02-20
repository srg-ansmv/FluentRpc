using System.Net;
using FluentRpc.Common;
using FluentRpc.Connections.Abstraction;

namespace FluentRpc.Server.Abstraction;

public interface IServer<TConnection>
    where TConnection : IConnection
{
    ValueTask<Result> Listen(EndPoint endPoint, CancellationToken cancellationToken = default);
    Task<Result<TConnection>> AcceptClient(CancellationToken cancellationToken = default);
}