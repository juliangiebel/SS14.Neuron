using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Neuron.OpenId.Services.Interfaces;
using Neuron.OpenId.Types;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Void = Neuron.OpenId.Types.Void;

namespace Neuron.OpenId.Services;

public class OpenIdActionService : IOpenIdActionService
{
    public const string ApplicationNotFoundError = "application_not_found";
    private const string IgnoreChallengeKey = "IgnoreAuthenticationChallenge";

    private readonly ISignedInIdentityService _signedInIdentity;
    private readonly IIdentityClaimsProvider _claimsProvider;
    private readonly ApplicationAuthorizationService _applicationAuthorizationService;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly ILogger<OpenIdActionService> _logger;

    public OpenIdActionService(ISignedInIdentityService signedInIdentity, IIdentityClaimsProvider claimsProvider, IOpenIddictApplicationManager applicationManager, IOpenIddictAuthorizationManager authorizationManager, ApplicationAuthorizationService applicationAuthorizationService, ILogger<OpenIdActionService> logger)
    {
        _signedInIdentity = signedInIdentity;
        _claimsProvider = claimsProvider;
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _applicationAuthorizationService = applicationAuthorizationService;
        _logger = logger;
    }

    public Result<Void, AuthenticationValidationFailure> ValidateOpenIdAuthentication(HttpContext context, AuthenticateResult auth, OpenIddictRequest request)
    {
        if (auth.Succeeded)
            return Result<Void, AuthenticationValidationFailure>.Success(Void.Nothing);
        
        var ignoreChallenge = context.Session.GetString(IgnoreChallengeKey);

        if (auth.Succeeded
            && !request.HasPromptValue(OpenIddictConstants.PromptValues.Login)
            && request.MaxAge is not 0
            && (request.MaxAge is null || auth.Properties?.IssuedUtc is null || TimeProvider.System.GetUtcNow() - auth.Properties.IssuedUtc < TimeSpan.FromSeconds(request.MaxAge.Value))
            && ignoreChallenge is null or "false")
        {
            return Result<Void, AuthenticationValidationFailure>.Success(Void.Nothing);
        }

        if (request.HasPromptValue(OpenIddictConstants.PromptValues.None))
            return Result<Void, AuthenticationValidationFailure>.Failure(AuthenticationValidationFailure.LoginRequired);
        
        context.Session.SetString(IgnoreChallengeKey, "true");
        var properties = new AuthenticationProperties
        {
            RedirectUri = context.Request.PathBase + context.Request.Path + QueryString.Create(
                context.Request.HasFormContentType ? context.Request.Form : context.Request.Query)
        };

        return Result<Void, AuthenticationValidationFailure>.Failure(AuthenticationValidationFailure.Challenge(properties));
    }

    public async Task<AuthorizationResult> AuthorizeActionAsync(OpenIddictRequest request, ImmutableArray<string> scopes)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!);
        if (application is null)
            return AuthorizationResult.Error("",  "application_not_found");
        
        var appName =  await _applicationManager.GetDisplayNameAsync(application);
        
        if (!await _signedInIdentity.IsAvailableAsync())
        {
            _logger.LogWarning("Signed in user is not available.");
            return AuthorizationResult.Forbidden(appName ?? "", OpenIddictConstants.Errors.AccessDenied);
        }
        
        var authorizations = new List<object>();

        // Ensure the user will be prompted if a prompt was requested later in the switch statement below
        if (!request.HasPromptValue(OpenIddictConstants.PromptValues.Consent))
        {
            authorizations = await _authorizationManager.FindAsync(
                subject: await _signedInIdentity.GetUserIdAsync(),
                client: await _applicationManager.GetIdAsync(application),
                status: OpenIddictConstants.Statuses.Valid,
                type: OpenIddictConstants.AuthorizationTypes.Permanent,
                scopes: scopes
            ).ToListAsync();
        }

        
        return await _applicationManager.GetConsentTypeAsync(application) switch
        {
            OpenIddictConstants.ConsentTypes.External when authorizations.Count is 0 => 
               AuthorizationResult.Forbidden(appName ?? "",  OpenIddictConstants.Errors.AccessDenied),

            OpenIddictConstants.ConsentTypes.Implicit or OpenIddictConstants.ConsentTypes.External when authorizations.Count is not 0 =>
                await HandleSignIn(request, application, authorizations),
            
            OpenIddictConstants.ConsentTypes.Explicit or OpenIddictConstants.ConsentTypes.Systematic when request.HasPromptValue(OpenIddictConstants.PromptValues.None) =>
                AuthorizationResult.Forbidden(appName ?? "",  OpenIddictConstants.Errors.ConsentRequired),
            
            _ => AuthorizationResult.Consent(appName ?? "", scopes)
        };
    }

    public async Task<ConsentResult> AcceptActionAsync(OpenIddictRequest request, ImmutableArray<string> scopes)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!);
        if (application is null)
            return ConsentResult.Error(ApplicationNotFoundError);
        
        if (!await _signedInIdentity.IsAvailableAsync())
        {
            _logger.LogWarning("Signed in user is not available.");
            return ConsentResult.Forbid(OpenIddictConstants.Errors.AccessDenied);
        }
        
        var userId = await _signedInIdentity.GetUserIdAsync();
        var authorizations = await _authorizationManager.FindAsync(
            subject: userId,
            client: await _applicationManager.GetIdAsync(application),
            status: OpenIddictConstants.Statuses.Valid,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: scopes
        ).ToListAsync();
        
        if (authorizations.Count is 0 && await _applicationManager.HasConsentTypeAsync(application, OpenIddictConstants.ConsentTypes.External))
            return ConsentResult.Forbid(OpenIddictConstants.Errors.AccessDenied);
        
        var principal = await _applicationAuthorizationService.CreateAuthorizedPrincipal(
            userId!, 
            application, 
            authorizations, 
            scopes, 
            _claimsProvider.GetDestinations);

        return ConsentResult.SignIn(principal);
    }

    public async Task<ExchangeResult> ExchangeActionAsync(OpenIddictRequest request, HttpContext context, ImmutableArray<string> scopes)
    {
        if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType()) 
            return ExchangeResult.Error(OpenIddictConstants.Errors.UnsupportedGrantType);
        
        var result = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var userId = result.Principal?.GetClaim(OpenIddictConstants.Claims.Subject);
        
        if (userId is null 
            || !await _signedInIdentity.IsAvailableAsync(userId) 
            || !await _signedInIdentity.CanSignInAsync(userId))
        {
            return ExchangeResult.Forbid(OpenIddictConstants.Errors.InvalidGrant);
        }
            
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);
        
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