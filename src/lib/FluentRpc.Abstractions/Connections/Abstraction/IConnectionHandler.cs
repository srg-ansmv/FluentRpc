using FluentRpc.Common;

namespace FluentRpc.Connections.Abstraction;

public interface IConnectionHandler<in TConnection>
    where TConnection : IConnection
{
    Task<UnitResult> Handle(TConnection connection, CancellationToken cancellationToken = default);
}