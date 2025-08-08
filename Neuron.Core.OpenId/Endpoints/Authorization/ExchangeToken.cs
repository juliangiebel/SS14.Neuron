using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Neuron.OpenId.Services;
using Neuron.OpenId.Services.Interfaces;
using Neuron.OpenId.Types;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Neuron.Core.OpenId.Endpoints.Authorization;

public static class ExchangeToken
{
    public static async Task<Results<BadRequest<string>, InternalServerError<string>, ForbidHttpResult, SignInHttpResult>> Post(
        HttpContext context, 
        [FromServices] IOpenIdActionService actionService)
    {
        var request = context.GetOpenIddictServerRequest();
        if (request is null)
            return TypedResults.BadRequest("The OpenID Connect request cannot be retrieved.");

        var result = await actionService.ExchangeActionAsync(request, context, request.GetScopes());

        return result.Type switch
        {
            ExchangeResult.ResultType.Error => 
                TypedResults.InternalServerError(Authorize.GetErrorDescription(result.ErrorName)),
            
            ExchangeResult.ResultType.Forbid => 
                AuthResults.Forbid(result.ErrorName ?? "", Authorize.GetErrorDescription(result.ErrorName)),
            
            _ => TypedResults.SignIn(result.Principal!, authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)
        };
    }
}