using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Neuron.Core.OpenId.Services.Interfaces;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Neuron.Core.OpenId.Services;

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


    /// <summary>
    /// Creates an authorized <see cref="ClaimsPrincipal"/> based on the provided user, application, authorizations, and scopes.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for which the principal is being created.</param>
    /// <param name="application">The application object representing the client making the request.</param>
    /// <param name="authorizations">A list of previously granted authorization objects related to the application and user.</param>
    /// <param name="scopes">An immutable array of scopes requested by the client application and approved for the user.</param>
    /// <param name="destinationsSelector">A function that determines the claim destinations for each <see cref="Claim"/>.</param>
    /// <returns>A task that represents the asynchronous operation and resolves to a <see cref="ClaimsPrincipal"/> containing the user's authorized identity.</returns>
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