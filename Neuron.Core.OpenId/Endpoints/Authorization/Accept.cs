using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Neuron.Common.Model;
using Neuron.OpenId.Helpers;
using Neuron.OpenId.Services;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Neuron.Core.OpenId.Endpoints.Authorization;

public static class Accept
{
    public static async Task<Results<ForbidHttpResult, SignInHttpResult>> Post(
        HttpContext context,
        UserManager<IdpUser> userManager,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        ApplicationAuthorizationService service)
    {//TODO: Check submit.accept and add antiforgery token
        var request = context.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var user = await userManager.GetUserAsync(context.User) ??
            throw new InvalidOperationException("The user cannot be retrieved.");
        
        var application = await applicationManager.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("The application cannot be retrieved.");
        
        var authorizations = await authorizationManager.FindAsync(
            subject: await userManager.GetUserIdAsync(user),
            client: await applicationManager.GetIdAsync(application),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()
        ).ToListAsync();
        
        if (authorizations.Count is 0 && await applicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
            return AuthResults.Forbid(Errors.ConsentRequired, "The logged in user is not allowed to access this client application");
        
    
        var userId = await userManager.GetUserIdAsync(user);
        var scopes = request.GetScopes();
        var principal = await service.CreateAuthorizedPrincipal(
            userId, 
            application, 
            authorizations, 
            scopes, 
            GetDestinations, 
            await SetClaims(userManager, user));
        
        return TypedResults.SignIn(principal, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IEnumerable<Claim>> SetClaims(UserManager<IdpUser> userManager, IdpUser user)
    {
        var username = await userManager.GetUserNameAsync(user) ?? "";
        
        return
        [
            new Claim(Claims.Subject, await userManager.GetUserIdAsync(user)),
            new Claim(Claims.Email, await userManager.GetEmailAsync(user) ?? ""),
            new Claim(Claims.Name, username),
            new Claim(Claims.PreferredUsername, await userManager.GetUserNameAsync(user) ?? username),
            ..ClaimHelper.FromList(Claims.Role, await userManager.GetRolesAsync(user))
        ];
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
}