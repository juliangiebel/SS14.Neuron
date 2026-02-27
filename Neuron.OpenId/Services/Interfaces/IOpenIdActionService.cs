using System.Collections.Immutable;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Neuron.OpenId.Types;
using OpenIddict.Abstractions;
using Void = Neuron.OpenId.Types.Void;

namespace Neuron.OpenId.Services.Interfaces;

public interface IOpenIdActionService
{
    /// <summary>
    /// Validates the current authentication state for an OpenID Connect request and determines whether
    /// the request can continue, must fail silently, or requires an interactive authentication challenge.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="auth">The authentication result for the current request.</param>
    /// <param name="request">The OpenIddict request (e.g., prompt/max_age).</param>
    /// <returns>
    /// A successful result when authentication is enough. Otherwise, a failure describing either
    /// a login requirement (e.g., <c>prompt=none</c>) or a challenge with redirect properties.
    /// </returns>
    public Result<Void, AuthenticationValidationFailure> ValidateOpenIdAuthentication(HttpContext context, AuthenticateResult auth, OpenIddictRequest request);
    
    /// <summary>
    /// Processes an authorization request and decides whether to sign in immediately, require user consent,
    /// or deny the request based on the application's consent type, prompt values, and existing authorizations.
    /// </summary>
    /// <param name="request">The OpenIddict authorization request.</param>
    /// <param name="scopes">The requested scopes.</param>
    /// <returns>An <see cref="AuthorizationResult"/> representing SignIn/Consent/Forbidden/Error.</returns>
    public Task<AuthorizationResult> AuthorizeActionAsync(OpenIddictRequest request, ImmutableArray<string> scopes);
    
    /// <summary>
    /// Accepts the consent decision and returns a sign-in result containing an authorized principal for token issuance.
    /// </summary>
    /// <param name="request">The OpenIddict authorization request.</param>
    /// <param name="scopes">The approved scopes.</param>
    /// <returns>A <see cref="ConsentResult"/> representing SignIn/Forbid/Error.</returns>
    public Task<ConsentResult> AcceptActionAsync(OpenIddictRequest request, ImmutableArray<string> scopes);
    
    /// <summary>
    /// Handles token exchange for the authorization code and refresh token grants and issues a new identity populated with claims.
    /// </summary>
    /// <param name="request">The OpenIddict token request.</param>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="scopes">The requested scopes.</param>
    /// <returns>An <see cref="ExchangeResult"/> representing SignIn/Forbid/Error.</returns>
    public Task<ExchangeResult> ExchangeActionAsync(OpenIddictRequest request, HttpContext context, ImmutableArray<string> scopes);
}