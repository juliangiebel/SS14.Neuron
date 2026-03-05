using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Neuron.Core.OpenId.Endpoints.Debug;

public static class GenerateCerts
{
    [UnsupportedOSPlatform("browser")]
    public static FileContentHttpResult GetEncryptionCert()
    {
        using var algorithm = RSA.Create(keySizeInBits: 2048);

        var subject = new X500DistinguishedName("CN=SS14.Neuron Server Encryption Certificate");
        var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment, critical: true));

        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(10));

        var bytes = certificate.Export(X509ContentType.Pfx, string.Empty);
        return TypedResults.File(bytes, "application/x-pkcs12", "server-encryption-certificate.pfx");
    }
    
    [UnsupportedOSPlatform("browser")]
    public static FileContentHttpResult GetSigningCert()
    {
        using var algorithm = RSA.Create(keySizeInBits: 2048);

        var subject = new X500DistinguishedName("CN=SS14.Neuron Server Signing Certificate");
        var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(10));

        var bytes = certificate.Export(X509ContentType.Pfx, string.Empty);
        return TypedResults.File(bytes, "application/x-pkcs12", "server-signing-certificate.pfx");
    }
}