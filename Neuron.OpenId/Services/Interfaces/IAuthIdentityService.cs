using System.Security.Claims;

namespace Neuron.Core.OpenId.Services.Interfaces;

public interface IIdentityClaimsService
{
    Task<string> GetUserIdAsync();
    Task<IEnumerable<Claim>> GetClaimsAsync();
    Task<bool> CanSignInAsync();
}