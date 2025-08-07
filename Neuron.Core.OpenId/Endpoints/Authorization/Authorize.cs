using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Neuron.Common.Model;
using Neuron.Core.OpenId.Components;
using Neuron.Core.OpenId.Helpers;
using Neuron.Core.OpenId.Services;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
// *Manic screaming*
using AuthResult = Microsoft.AspNetCore.Http.HttpResults.Results<Microsoft.AspNetCore.Http.HttpResults.RazorComponentResult, Microsoft.AspNetCore.Http.HttpResults.ForbidHttpResult, Microsoft.AspNetCore.Http.HttpResults.ChallengeHttpResult, Microsoft.AspNetCore.Http.HttpResults.SignInHttpResult>;

namespace Neuron.Core.OpenId.Endpoints.Authorization;

public static class Authorize
{
    private const string IgnoreChallengeKey = "IgnoreAuthenticationChallenge";
    
    public static async Task<AuthResult> GetAndPost(
            HttpContext context,
            UserManager<IdpUser> userManager,
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictAuthorizationManager authorizationManager,
            ApplicationAuthorizationService service,
            IOpenIddictScopeManager scopeManager)
    {
        var request = context.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await context.AuthenticateAsync();
        if (TryValidateAuthentication(context, result, request, out var response))
            return response;

        var user = await userManager.GetUserAsync(result.Principal!)
                   ?? throw new InvalidOperationException("The user cannot be retrieved.");

        var application = await applicationManager.FindByClientIdAsync(request.ClientId!)
                          ?? throw new InvalidOperationException(
                              "The application details cannot be found in the database.");

        var authorizations = new List<object>();

        // Ensure the user will be prompted if a prompt was requested later in the switch statement below
        if (!request.HasPromptValue(PromptValues.Consent))
        {
            authorizations = await authorizationManager.FindAsync(
                subject: await userManager.GetUserIdAsync(user),
                client: await applicationManager.GetIdAsync(application),
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: request.GetScopes()
            ).ToListAsync();
        }
        
        return await applicationManager.GetConsentTypeAsync(application) switch
        {
            ConsentTypes.External when authorizations.Count is 0 => 
                AuthResults.Forbid(Errors.ConsentRequired, "The logged in user is not allowed to access this client application"),

            ConsentTypes.Implicit or ConsentTypes.External when authorizations.Count is not 0 =>
                await HandleSignIn(service, request, user, application, userManager, authorizations),
            
            ConsentTypes.Explicit or ConsentTypes.Systematic when request.HasPromptValue(PromptValues.None) =>
                AuthResults.Forbid(Errors.ConsentRequired, "Interactive user consent is required."),
            
            _ => new RazorComponentResult<ConsentPage>(new Model(await applicationManager.GetDisplayNameAsync(application), request.GetScopes()))
        };

    }

    private static async Task<AuthResult> HandleSignIn(
            ApplicationAuthorizationService service,
            OpenIddictRequest request,
            IdpUser user,
            object application,
            UserManager<IdpUser> userManager,
            List<object> authorizations)
    {
        var scopes = request.GetScopes();
        var userId = await userManager.GetUserIdAsync(user);
        var identity = await service.CreateAuthorizedPrincipal(userId, application, authorizations, scopes, GetDestinations);
        return TypedResults.SignIn(
            new ClaimsPrincipal(identity),
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static IEnumerable<string> GetDestinations(Claim claim) => claim.Type switch
    {
        Claims.Name or Claims.PreferredUsername => DestinationHelper.Destination(claim, Scopes.Profile),
        Claims.Email => DestinationHelper.Destination(claim, Scopes.Email),
        Claims.Role => DestinationHelper.Destination(claim, Scopes.Roles),
        Claims.Address => DestinationHelper.Destination(claim, Scopes.Address),
        "AspNet.Identity.SecurityStamp" => DestinationHelper.Destination(),
        _ => DestinationHelper.Destination(Destinations.AccessToken)
    };

    private static bool TryValidateAuthentication(
        HttpContext context, 
        AuthenticateResult result, 
        OpenIddictRequest request, 
        [NotNullWhen(true)] out AuthResult? response)
    {
        if (result.Succeeded)
        {
            response = null;
            return false;
        }

        var ignoreChallenge = context.Session.GetString(IgnoreChallengeKey);

        if (result.Succeeded
            && !request.HasPromptValue(PromptValues.Login)
            && request.MaxAge is not 0
            && (request.MaxAge is null || result.Properties?.IssuedUtc is null || TimeProvider.System.GetUtcNow() - result.Properties.IssuedUtc < TimeSpan.FromSeconds(request.MaxAge.Value))
            && ignoreChallenge is null or "false")
        {
            response = null;
            return false;
        }

        if (request.HasPromptValue(PromptValues.None))
        {
            response = AuthResults.Forbid(Errors.LoginRequired, "The login is required.");
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

    public sealed record Model(string? ApplicationName, ImmutableArray<string>? Scopes);
}