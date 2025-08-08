using System.Security.Claims;

namespace Neuron.OpenId.Services.Interfaces;

public interface ISignedInIdentityService
{
    Task<bool> IsAvailableAsync();
    Task<string?> GetUserIdAsync();
    Task<bool> CanSignInAsync();
    
    Task<bool> IsAvailableAsync(string userId);
    Task<string?> GetUserIdAsync(string userId);
    Task<bool> CanSignInAsync(string userId);
}