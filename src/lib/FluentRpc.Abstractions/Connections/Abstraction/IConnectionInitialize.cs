using FluentRpc.Common;

namespace FluentRpc.Connections.Abstraction;

public interface IConnectionInitialize
{
    Task<UnitResult> InitializeAsync(CancellationToken cancellationToken = default);
}