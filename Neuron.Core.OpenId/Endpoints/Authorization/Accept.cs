using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Neuron.Common.Model;
using Neuron.OpenId.Helpers;
using Neuron.OpenId.Services;
using Neuron.OpenId.Services.Interfaces;
using Neuron.OpenId.Types;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Neuron.Core.OpenId.Endpoints.Authorization;

public static class Accept
{
    public static async Task<Results<ForbidHttpResult, SignInHttpResult, BadRequest<string>>> Post(
        HttpContext context, 
        [FromServices] IOpenIdActionService actionService)
    {
        if (!context.Request.Form.ContainsKey("submit.accept"))
            return TypedResults.BadRequest("Invalid form data");
        
        var request = context.GetOpenIddictServerRequest();
        if (request is null)
            return TypedResults.BadRequest("The OpenID Connect request cannot be retrieved.");
        
        var result = await actionService.AcceptActionAsync(request, request.GetScopes());

        return result.Type switch
        {
            ConsentResult.ResultType.SignIn =>
                TypedResults.SignIn(result.Principal!,
                    authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme),
            ConsentResult.ResultType.Forbid =>
                AuthResults.Forbid(result.ErrorName ?? "", Authorize.GetErrorDescription(result.ErrorName)),
            _ => TypedResults.BadRequest(Authorize.GetErrorDescription(result.ErrorName))
        };
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