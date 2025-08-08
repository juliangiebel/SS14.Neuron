using System.Collections.Immutable;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Neuron.OpenId.Types;
using OpenIddict.Abstractions;
using Void = Neuron.OpenId.Types.Void;

namespace Neuron.OpenId.Services.Interfaces;

public interface IOpenIdActionService
{
    public Result<Void, AuthenticationValidationFailure> ValidateOpenIdAuthentication(HttpContext context, AuthenticateResult auth, OpenIddictRequest request);
    public Task<AuthorizationResult> AuthorizeActionAsync(OpenIddictRequest request, ImmutableArray<string> scopes);
}