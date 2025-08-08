using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Neuron.OpenId.Services;

public class ApplicationAuthorizationService
{
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictApplicationManager _applicationManager;

    public ApplicationAuthorizationService(IOpenIddictScopeManager scopeManager, IOpenIddictAuthorizationManager authorizationManager, IOpenIddictApplicationManager applicationManager)
    {
        _scopeManager = scopeManager;
        _authorizationManager = authorizationManager;
        _applicationManager = applicationManager;
    }


    public async Task<ClaimsPrincipal> CreateAuthorizedPrincipal(
        string userId, object application,
        List<object> authorizations,
        ImmutableArray<string> scopes,
        Func<Claim, IEnumerable<string>> destinationsSelector, 
        IEnumerable<Claim>? claims = null)
    {
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        if (claims is not null)
            identity.AddClaims(claims);
        
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