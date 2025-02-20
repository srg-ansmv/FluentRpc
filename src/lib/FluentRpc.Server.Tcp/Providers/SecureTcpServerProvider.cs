using FluentRpc.Certificates.Abstractions;
using FluentRpc.Common;
using FluentRpc.Providers.Abstraction;
using FluentRpc.Tcp.Connections;
using FluentRpc.Tcp.Server;

namespace FluentRpc.Tcp.Providers;

public class SecureTcpServerProvider: IServerProvider<ServerSslTcpConnection, SecureTcpServer, SecureTcpServerResult>
{
    private readonly ICertificateProvider _certificateProvider;

    public SecureTcpServerProvider(ICertificateProvider certificateProvider)
    {
        _certificateProvider = certificateProvider;
    }

    public ValueTask<Result<SecureTcpServerResult>> Create()
    {
        try
        {
            var res = new SecureTcpServerResult { Server = new SecureTcpServer(_certificateProvider) };
            return ValueTask.FromResult(Result<SecureTcpServerResult>.Ok(res));
        }
        catch (Exception e)
        {
            return ValueTask.FromResult(Result<SecureTcpServerResult>.Failure(e));
        }
    }
}

public class SecureTcpServerResult : IServerResult<SecureTcpServer, ServerSslTcpConnection>
{
    public required SecureTcpServer Server { get; init; }
}
