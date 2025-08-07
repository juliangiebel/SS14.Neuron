using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Server.AspNetCore.OpenIddictServerAspNetCoreConstants;

namespace Neuron.Core.OpenId.Endpoints;

public static class AuthResults
{
    public static ForbidHttpResult Forbid(string error, string description) => TypedResults.Forbid(
        authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
        properties: new AuthenticationProperties(new Dictionary<string, string?>
        {
            [Properties.Error] = error,
            [Properties.ErrorDescription] = description,
        }));
}