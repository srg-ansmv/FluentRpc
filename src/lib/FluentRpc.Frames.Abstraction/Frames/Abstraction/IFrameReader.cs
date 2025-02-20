using FluentRpc.Common;
using FluentRpc.Connections.Abstraction;

namespace FluentRpc.Frames.Abstraction;

public interface IFrameReader<TFrame, TConnection, in TFrameReadOptions>
    where TConnection : IConnection
    where TFrameReadOptions : IFrameReadOptions<TConnection>
{
    Task<Result<TFrame>> ReadAsync(TFrameReadOptions frameReadOptions, CancellationToken cancellationToken = default);
}

public interface IFrameReadOptions<out TConnection>
{
    TConnection Connection { get; }
}