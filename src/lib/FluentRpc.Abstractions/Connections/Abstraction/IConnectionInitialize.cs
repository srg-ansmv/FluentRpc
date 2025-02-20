using FluentRpc.Common;

namespace FluentRpc.Connections.Abstraction;

public interface IConnectionInitialize
{
    Task<Result> InitializeAsync(CancellationToken cancellationToken = default);
}