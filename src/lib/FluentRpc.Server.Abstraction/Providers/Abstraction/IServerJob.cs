using FluentRpc.Common;

namespace FluentRpc.Providers.Abstraction;

public interface IServerJob
{
    Task<UnitResult> StartAsync(CancellationToken cancellationToken = default);
    Task<UnitResult> StopAsync(CancellationToken cancellationToken = default);
}