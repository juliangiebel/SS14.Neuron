namespace Neuron.OpenId.Services.Interfaces;

public interface ISignedInIdentityService
{
    Task<bool> IsAvailableAsync();
    Task<string> GetUserIdAsync();
    Task<bool> CanSignInAsync();
}