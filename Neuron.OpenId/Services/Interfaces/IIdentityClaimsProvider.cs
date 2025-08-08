using System.Security.Claims;

namespace Neuron.OpenId.Services.Interfaces;

public interface IIdentityClaimsProvider
{
    Task ProvideClaimsAsync(ClaimsIdentity identity);
}