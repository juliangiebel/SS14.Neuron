using System.Collections.Immutable;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Neuron.Common.Model;
using Neuron.Core.OpenId.Components;
using Neuron.OpenId.Services;
using Neuron.OpenId.Types;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Neuron.Core.OpenId.Endpoints.Authorization;

public static class Authorize
{
    public static async Task<IResult> GetAndPost(
            HttpContext context,
            OpenIdActionService actionService)
    {
        var request = context.GetOpenIddictServerRequest();
        if (request is null)
            return TypedResults.BadRequest("The OpenID Connect request cannot be retrieved.");

        var result = await context.AuthenticateAsync();
        var validation = actionService.ValidateOpenIdAuthentication(context, result, request);
        
        switch (validation.IsSuccess)
        {
            case false when validation.Error.IsChallenge:
                return TypedResults.Challenge(validation.Error.Properties);
            case false:
                return AuthResults.Forbid(validation.Error.Error!, "The login is required.");
        }

        var authorization = await actionService.AuthorizeActionAsync(request, request.GetScopes());
        
        return authorization.Type switch
        {
            AuthorizationResult.ResultType.Forbidden => 
                AuthResults.Forbid(authorization.ErrorName ?? "", GetErrorDescription(authorization.ErrorName)),

            AuthorizationResult.ResultType.Error =>
                TypedResults.InternalServerError(GetErrorDescription(authorization.ErrorName)),
            
            AuthorizationResult.ResultType.SignIn =>
                TypedResults.SignIn(authorization.Principal!, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme),
            
            _ => new RazorComponentResult<ConsentPage>(new Model(authorization.AppName, authorization.Scopes))
        };

    }

    // TODO: Use translation instead
    private static string GetErrorDescription(string? error) => error switch
    {
        OpenIdActionService.ApplicationNotFoundError => "No application for the given client id.",
        Errors.AccessDenied => "The logged in user is not allowed to access this client application.",
        Errors.ConsentRequired => "Interactive user consent is required.",
        _ => "An unknown error occurred."
    };
    
    public sealed record Model(string? ApplicationName, ImmutableArray<string>? Scopes);
}