using FluentRpc.Common;
using FluentRpc.Connections.Abstraction;
using FluentRpc.Server.Abstraction;

namespace FluentRpc.Providers.Abstraction;

public abstract class BaseServerJob<TServerProvider, TConnection, TServer, TServerResult> : IServerJob
    where TServerProvider : IServerProvider<TConnection, TServer, TServerResult>
    where TServerResult : IServerResult<TServer, TConnection>
    where TServer : IServer<TConnection>
    where TConnection : IConnection
{
    private readonly TServerProvider _serverProvider;
    private CancellationTokenSource? _cancellationTokenSource;

    protected BaseServerJob(TServerProvider serverProvider)
    {
        _serverProvider = serverProvider;
    }


    public virtual async Task<UnitResult> StartAsync(CancellationToken cancellationToken = default)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var server = await _serverProvider.Create();
        if (!server.IsOk(out var value))
        {
            if (server.IsError(out var error)) error.Value.Throw();
            if (server.IsFailure(out var failure)) throw failure;
        }

        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            await ProcessTick(value!, _cancellationTokenSource.Token);
        }

        return UnitResult.Ok;
    }

    public virtual async Task<UnitResult> StopAsync(CancellationToken cancellationToken = default)
    {
        using (_cancellationTokenSource)
        {
            if (_cancellationTokenSource is null)
            {
                return UnitResult.Error(new Error("ServerNotStarted", "Server not started"));
            }

            await _cancellationTokenSource.CancelAsync().WaitAsync(cancellationToken);
            return UnitResult.Ok;
        }
    }

    protected abstract Task<UnitResult> ProcessTick(TServerResult serverResult, CancellationToken cancellationToken);
}

public abstract class BaseServerJob<TServerProvider, TConnection, TServer>
    : BaseServerJob<TServerProvider, TConnection, TServer, IServerResult<TServer, TConnection>>
    where TServerProvider : IServerProvider<TConnection, TServer>
    where TServer : IServer<TConnection>
    where TConnection : IConnection
{
    protected BaseServerJob(TServerProvider serverProvider) : base(serverProvider)
    {
    }
}

public abstract class BaseServerJob<TServerProvider, TConnection>
    : BaseServerJob<TServerProvider, TConnection, IServer<TConnection>>
    where TServerProvider : IServerProvider<TConnection>
    where TConnection : IConnection
{
    protected BaseServerJob(TServerProvider serverProvider) : base(serverProvider)
    {
    }
}

public abstract class BaseServerJob<TServerProvider>
    : BaseServerJob<TServerProvider, IConnection>
    where TServerProvider : IServerProvider<IConnection>
{
    protected BaseServerJob(TServerProvider serverProvider) : base(serverProvider)
    {
    }
}

public abstract class BaseServerJob : BaseServerJob<IServerProvider>
{
    protected BaseServerJob(IServerProvider serverProvider) : base(serverProvider)
    {
    }
}