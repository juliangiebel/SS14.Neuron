using System.Collections.Immutable;
using System.Security.Claims;
using Neuron.OpenId.Helpers;
using OpenIddict.Abstractions;

namespace Neuron.OpenId.Services.Interfaces;

public interface IIdentityClaimsProvider
{
    /// <summary>
    /// Provides claims for the specified identity.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="scopes"></param>
    /// <param name="identity">The identity for which claims are to be provided.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ProvideClaimsAsync(string userId, ImmutableArray<string> scopes, ClaimsIdentity identity);

    /// <summary>
    /// TODO: Document
    /// 
    /// </summary>
    /// <param name="claim"></param>
    /// <returns></returns>
    IEnumerable<string> GetDestinations(Claim claim)  => claim.Type switch
    {
        OpenIddictConstants.Claims.Name or OpenIddictConstants.Claims.PreferredUsername => DestinationHelper.Destination(claim, OpenIddictConstants.Scopes.Profile),
        OpenIddictConstants.Claims.Email => DestinationHelper.Destination(claim, OpenIddictConstants.Scopes.Email),
        OpenIddictConstants.Claims.Role => DestinationHelper.Destination(claim, OpenIddictConstants.Scopes.Roles),
        OpenIddictConstants.Claims.Address => DestinationHelper.Destination(claim, OpenIddictConstants.Scopes.Address),
        "AspNet.Identity.SecurityStamp" => DestinationHelper.Destination(),
        _ => DestinationHelper.Destination(OpenIddictConstants.Destinations.AccessToken)
    };
}