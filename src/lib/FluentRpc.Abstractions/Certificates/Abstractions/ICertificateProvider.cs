using System.Security.Cryptography.X509Certificates;
using FluentRpc.Common;

namespace FluentRpc.Certificates.Abstractions;

public interface ICertificateProvider
{
    Task<Result<X509Certificate>> ProvideAsync(CancellationToken cancellationToken = default);
}