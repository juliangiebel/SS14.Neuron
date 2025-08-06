using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Neuron.Common.Model;
using Neuron.Core.OpenId.Components;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Neuron.Core.OpenId.Endpoints;

public static class Consent
{
    private const string IgnoreChallengeKey = "IgnoreAuthenticationChallenge";
    private const string ConsentKey = "consent";
    
    public static async Task<Results<RazorComponentResult, ForbidHttpResult, ChallengeHttpResult>> Get(
        HttpContext context,
        UserManager<IdpUser> userManager,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager
        )
    {
        var request = context.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await context.AuthenticateAsync();
        if (TryValidateAuthentication(context, result, request, out var response))
            return response;
        
        var user = await userManager.GetUserAsync(result.Principal!) 
                   ?? throw new InvalidOperationException("The user cannot be retrieved.");
        
        var application = await applicationManager.FindByClientIdAsync(request.ClientId!)
            ?? throw new InvalidOperationException("The application details cannot be found in the database.");

        var authorizations = await authorizationManager.FindAsync(
            subject: await userManager.GetUserIdAsync(user),
            client: await applicationManager.GetIdAsync(application),
            status: OpenIddictConstants.Statuses.Valid,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: request.GetScopes()
        ).ToListAsync();

        switch (await applicationManager.GetConsentTypeAsync(application))
        {
            default: return new RazorComponentResult<ConsentPage>();
        }
    }

    private static bool TryValidateAuthentication(
        HttpContext context, 
        AuthenticateResult result, 
        OpenIddictRequest request, 
        [NotNullWhen(true)] out Results<RazorComponentResult, ForbidHttpResult, ChallengeHttpResult>? response)
    {
        if (result.Succeeded)
        {
            response = null;
            return false;
        }

        var ignoreChallenge = context.Session.GetString(IgnoreChallengeKey);
        
        if (!(request.HasPromptValue(OpenIddictConstants.PromptValues.Login)
            || request.MaxAge is 0
            || request.MaxAge is not null && result.Properties?.IssuedUtc is not null &&
                TimeProvider.System.GetUtcNow() - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value) &&
            ignoreChallenge is null or "false"))
        {
            response = null;
            return false;
        }

        if (request.HasPromptValue(OpenIddictConstants.PromptValues.None))
        {
            response = TypedResults.Forbid(
                authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.LoginRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The login is required.",
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorUri] = "https://openid.net/specs/openid-connect-core-1_0.html#AuthError"
                }));
            
            return true;
        }
        
        context.Session.SetString(IgnoreChallengeKey, "true");
        
        response = TypedResults.Challenge(new AuthenticationProperties
        {
            RedirectUri = context.Request.PathBase + context.Request.Path + QueryString.Create(
                context.Request.HasFormContentType ? context.Request.Form : context.Request.Query)
        });
        
        return true;
    }
}