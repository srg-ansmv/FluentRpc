﻿using System.Net.Security;
using System.Net.Sockets;
using FluentRpc.Certificates.Abstractions;
using FluentRpc.Common;
using FluentRpc.Connections.Abstraction;

namespace FluentRpc.Tcp.Connections;

public class ServerSslTcpConnection : IConnection, IConnectionInitialize
{
    private readonly SslStream _sslStream;
    private readonly ICertificateProvider _certificateProvider;

    public ServerSslTcpConnection(Socket socket, ICertificateProvider certificateProvider)
    {
        _sslStream = new SslStream(new NetworkStream(socket, ownsSocket: true));
        _certificateProvider = certificateProvider;
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
        var certResult = await _certificateProvider.ProvideAsync(cancellationToken);

        return await certResult.FoldAsync(
            async ok =>
            {
                try
                {
                    await _sslStream.AuthenticateAsServerAsync(
                        ok,
                        clientCertificateRequired: false,
                        checkCertificateRevocation: true
                    );
                    return UnitResult.Ok;
                }
                catch (Exception e)
                {
                    return UnitResult.Failure(e);
                }
            },
            err => Task.FromResult(UnitResult.Error(err)),
            ex => Task.FromResult(UnitResult.Failure(ex))
        );
    }
}