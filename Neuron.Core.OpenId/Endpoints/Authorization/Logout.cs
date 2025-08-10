using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Neuron.Common.Model;
using Neuron.Core.OpenId.Components;
using OpenIddict.Server.AspNetCore;

namespace Neuron.Core.OpenId.Endpoints.Authorization;

public static class Logout
{
    public static async Task<RazorComponentResult> Get()
    {
        return new RazorComponentResult<LogoutPage>();
    }

    public static async Task<SignOutHttpResult> Post(SignInManager<IdpUser> signInManager)
    {
        await signInManager.SignOutAsync();

        // Returning a SignOutResult will ask OpenIddict to redirect the user agent
        // to the post_logout_redirect_uri specified by the client application or to
        // the RedirectUri specified in the authentication properties if none was set.
        return TypedResults.SignOut(
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
            properties: new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }
}