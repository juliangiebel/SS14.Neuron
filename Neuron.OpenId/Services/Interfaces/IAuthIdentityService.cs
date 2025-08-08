namespace Neuron.OpenId.Services.Interfaces;

public interface ISignedInIdentityService
{
    Task<string> GetUserIdAsync();
    Task<bool> CanSignInAsync();
}