using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Neuron.OpenId.Services.Interfaces;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Neuron.OpenId.Services;

public class ApplicationAuthorizationService
{
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IIdentityClaimsProvider _claimsProvider;

    public ApplicationAuthorizationService(IOpenIddictScopeManager scopeManager, IOpenIddictAuthorizationManager authorizationManager, IOpenIddictApplicationManager applicationManager, IIdentityClaimsProvider claimsProvider)
    {
        _scopeManager = scopeManager;
        _authorizationManager = authorizationManager;
        _applicationManager = applicationManager;
        _claimsProvider = claimsProvider;
    }


    public async Task<ClaimsPrincipal> CreateAuthorizedPrincipal(
        string userId, object application,
        List<object> authorizations,
        ImmutableArray<string> scopes,
        Func<Claim, IEnumerable<string>> destinationsSelector)
    {
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        await _claimsProvider.ProvideClaimsAsync(userId, scopes, identity);
        
        identity.SetScopes(scopes);
        identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

        var authorization = authorizations.LastOrDefault();
        authorization ??= await _authorizationManager.CreateAsync(
            identity: identity,
            subject: userId,
            client: (await _applicationManager.GetIdAsync(application))!,
            type: AuthorizationTypes.Permanent,
            scopes: identity.GetScopes()
        );

        identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
        identity.SetDestinations(destinationsSelector);
        return new ClaimsPrincipal(identity);
    }
}