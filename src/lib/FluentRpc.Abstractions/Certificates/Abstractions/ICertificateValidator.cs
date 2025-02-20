using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace FluentRpc.Certificates.Abstractions;

public interface ICertificateValidator
{
    bool Validate(X509Certificate? certificate, X509Chain? chain, SslPolicyErrors errors);
}