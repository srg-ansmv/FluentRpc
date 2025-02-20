using FluentRpc.Common;

namespace FluentRpc.Connections.Abstraction;

public interface IConnectionHandler<in TConnection>
    where TConnection : IConnection
{
    Task<Result> Handle(TConnection connection, CancellationToken cancellationToken = default);
}