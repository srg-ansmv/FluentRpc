using System.Net.Security;
using System.Net.Sockets;
using FluentRpc.Certificates.Abstractions;
using FluentRpc.Common;
using FluentRpc.Connections.Abstraction;
using Microsoft.Extensions.Options;

namespace FluentRpc.Tcp.Connections;

public class ClientSslTcpConnection : IConnection, IConnectionInitialize
{
    private readonly SslStream _sslStream;
    private readonly IOptionsSnapshot<ClientSslTcpConnectionOptions> _options;

    public ClientSslTcpConnection(
        Socket socket,
        ICertificateValidator certificateValidator,
        IOptionsSnapshot<ClientSslTcpConnectionOptions> options
    )
    {
        _options = options;
        _sslStream = new SslStream(
            new NetworkStream(socket, ownsSocket: true),
            false,
            (_, x, y, z) => certificateValidator.Validate(x, y, z),
            null
        );
    }

    public async Task<Result<ReadPacket>> ReceiveBytes(ReceiveBytesOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var owner = options.Pool?.Rent(options.BufferSize) ?? new NotPooledMemoryOwner(new byte[options.BufferSize]);
            var readCount = await _sslStream.ReadAsync(owner.Memory, cancellationToken);
            return Result<ReadPacket>.Ok(new(readCount, owner));
        }
        catch (Exception e)
        {
            return Result<ReadPacket>.Failure(e);
        }
    }

    public async Task<UnitResult> WriteBytes(WritePacket packet, CancellationToken cancellationToken = default)
    {
        try
        {
            await _sslStream.WriteAsync(packet.Memory, cancellationToken);
            return UnitResult.Ok;
        }
        catch (Exception e)
        {
            return UnitResult.Failure(e);
        }
    }

    public async Task<Result<ReadPacket>> ReceiveAtLeastBytes(
        ReceiveAtLeastBytesOptions options,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var bufferSize = options.AtLeast + options.RestBufferSize;
            var owner = options.Pool?.Rent(bufferSize) ?? new NotPooledMemoryOwner(new byte[bufferSize]);
            var readCount = await _sslStream.ReadAtLeastAsync(owner.Memory, options.AtLeast, true, cancellationToken);

            return Result<ReadPacket>.Ok(new(readCount, owner));
        }
        catch (Exception e)
        {
            return Result<ReadPacket>.Failure(e);
        }
    }

    public async Task<UnitResult> InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _sslStream.AuthenticateAsClientAsync(_options.Value.Server);
            return UnitResult.Ok;
        }
        catch (Exception e)
        {
            return UnitResult.Failure(e);
        }
    }
}

public class ClientSslTcpConnectionOptions
{
    public required string Server { get; init; }
}