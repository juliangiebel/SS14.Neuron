using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Neuron.Core.Identity.Model;

namespace Neuron.Core.Identity.Endpoints.Account;

public static class Logout
{
    public static async Task<RedirectHttpResult> Post(SignInManager<IdpUser> signInManager, [FromQuery] string? returnUrl = null)
    { 
        await signInManager.SignOutAsync(); 
        return TypedResults.LocalRedirect(returnUrl ?? "/");
    }
}