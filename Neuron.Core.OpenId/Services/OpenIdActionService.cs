using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Neuron.Core.OpenId.Database.model;
using Neuron.Core.OpenId.Services.Interfaces;
using Neuron.Core.OpenId.Types;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Void = Neuron.Core.OpenId.Types.Void;

namespace Neuron.Core.OpenId.Services;

/// <summary>
/// Encapsulates OpenIddict server-side OpenID Connect/OAuth2 endpoint logic (authorize, consent, exchange).
/// </summary>
public class OpenIdActionService : IOpenIdActionService
{
    /// <summary>
    /// Error code returned when the requested client application cannot be found.
    /// </summary>
    public const string ApplicationNotFoundError = "application_not_found";

    private readonly ISignedInIdentityService _signedInIdentity;
    private readonly IIdentityClaimsProvider _claimsProvider;
    private readonly ApplicationAuthorizationService _applicationAuthorizationService;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly OpenIddictApplicationManager<IdpApplication> _applicationManager;
    private readonly ILogger<OpenIdActionService> _logger;

    public OpenIdActionService(
        ISignedInIdentityService signedInIdentity, 
        IIdentityClaimsProvider claimsProvider, 
        OpenIddictApplicationManager<IdpApplication> applicationManager, 
        IOpenIddictAuthorizationManager authorizationManager, 
        ApplicationAuthorizationService applicationAuthorizationService, 
        ILogger<OpenIdActionService> logger)
    {
        _signedInIdentity = signedInIdentity;
        _claimsProvider = claimsProvider;
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _applicationAuthorizationService = applicationAuthorizationService;
        _logger = logger;
    }

    /// <inheritdoc />
    public Result<Void, AuthenticationValidationFailure> ValidateOpenIdAuthentication(
        HttpContext context,
        bool ignoreChallenge,
        AuthenticateResult auth,
        OpenIddictRequest request)
    {
        // Auth succeeded and nothing is forcing re-authentication
        if (auth.Succeeded
            && !request.HasPromptValue(PromptValues.Login)
            && request.MaxAge is not 0
            && (request.MaxAge is null || auth.Properties?.IssuedUtc is null || TimeProvider.System.GetUtcNow() - auth.Properties.IssuedUtc < TimeSpan.FromSeconds(request.MaxAge.Value)))
        {
            return Result<Void, AuthenticationValidationFailure>.Success(Void.Nothing);
        }

        if (ignoreChallenge || request.HasPromptValue(PromptValues.None))
            return Result<Void, AuthenticationValidationFailure>.Failure(AuthenticationValidationFailure.LoginRequired);

        var properties = new AuthenticationProperties
        {
            RedirectUri = context.Request.PathBase + context.Request.Path + QueryString.Create(
                context.Request.HasFormContentType ? context.Request.Form : context.Request.Query),
        };

        return Result<Void, AuthenticationValidationFailure>.Failure(AuthenticationValidationFailure.Challenge(properties));
    }
    
    /// <inheritdoc />
    public async Task<AuthorizationResult> AuthorizeActionAsync(OpenIddictRequest request, ImmutableArray<string> scopes)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!);
        if (application is null)
            return AuthorizationResult.Error(null,  ApplicationNotFoundError);
        
        if (!await _signedInIdentity.IsAvailableAsync())
        {
            _logger.LogWarning("Signed in user is not available.");
            return AuthorizationResult.Forbidden(application, Errors.AccessDenied);
        }
        
        var authorizations = new List<object>();

        // Ensure the user will be prompted if a prompt was requested later in the switch statement below
        if (!request.HasPromptValue(PromptValues.Consent))
        {
            authorizations = await _authorizationManager.FindAsync(
                subject: await _signedInIdentity.GetUserIdAsync(),
                client: await _applicationManager.GetIdAsync(application),
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: scopes
            ).ToListAsync();
        }

        
        return await _applicationManager.GetConsentTypeAsync(application) switch
        {
            ConsentTypes.External when authorizations.Count is 0 => 
               AuthorizationResult.Forbidden(application,  Errors.AccessDenied),

            ConsentTypes.Implicit or ConsentTypes.External when authorizations.Count is not 0 =>
                await HandleSignIn(request, application, authorizations),
            
            ConsentTypes.Explicit or ConsentTypes.Systematic when request.HasPromptValue(PromptValues.None) =>
                AuthorizationResult.Forbidden(application,  Errors.ConsentRequired),
            
            _ => AuthorizationResult.Consent(application, scopes)
        };
    }

    /// <inheritdoc />
    public async Task<ConsentResult> AcceptActionAsync(OpenIddictRequest request, ImmutableArray<string> scopes)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!);
        if (application is null)
            return ConsentResult.Error(ApplicationNotFoundError);
        
        if (!await _signedInIdentity.IsAvailableAsync())
        {
            _logger.LogWarning("Signed in user is not available.");
            return ConsentResult.Forbid(Errors.AccessDenied);
        }
        
        var userId = await _signedInIdentity.GetUserIdAsync();
        var authorizations = await _authorizationManager.FindAsync(
            subject: userId,
            client: await _applicationManager.GetIdAsync(application),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: scopes
        ).ToListAsync();
        
        if (authorizations.Count is 0 && await _applicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
            return ConsentResult.Forbid(Errors.AccessDenied);
        
        var principal = await _applicationAuthorizationService.CreateAuthorizedPrincipal(
            userId!, 
            application, 
            authorizations, 
            scopes, 
            _claimsProvider.GetDestinations);

        return ConsentResult.SignIn(principal);
    }

    /// <inheritdoc />
    public async Task<ExchangeResult> ExchangeActionAsync(OpenIddictRequest request, HttpContext context, ImmutableArray<string> scopes)
    {
        if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType()) 
            return ExchangeResult.Error(Errors.UnsupportedGrantType);
        
        var result = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var userId = result.Principal?.GetClaim(Claims.Subject);
        
        if (userId is null 
            || !await _signedInIdentity.IsAvailableAsync(userId) 
            || !await _signedInIdentity.CanSignInAsync(userId))
        {
            return ExchangeResult.Forbid(Errors.InvalidGrant);
        }
            
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);
        
        await _claimsProvider.ProvideClaimsAsync(await _signedInIdentity.GetUserIdAsync(userId) ?? "", scopes, identity);
        identity.SetDestinations(_claimsProvider.GetDestinations);
        return ExchangeResult.SignIn(new ClaimsPrincipal(identity));
    }

    private async Task<AuthorizationResult> HandleSignIn(
        OpenIddictRequest request,
        object application,
        List<object> authorizations)
    {
        var scopes = request.GetScopes();
        var userId = await _signedInIdentity.GetUserIdAsync();
        var identity = await _applicationAuthorizationService
            .CreateAuthorizedPrincipal(userId!, application, authorizations, scopes, _claimsProvider.GetDestinations);

        return AuthorizationResult.SignIn(identity);
    }
}