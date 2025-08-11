namespace Neuron.Core.OpenId.Configuration;

public class OpenIdCertificateConfiguration
{
    public const string Name = "Certificates";
    
    public string? EncryptionCertificatePath { get; set; }
    public string? EncryptionCertificatePassword { get; set; }
    
    public string? SigningCertificatePath { get; set; }
    public string? SigningCertificatePassword { get; set; }
}