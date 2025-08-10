using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Neuron.Common.Model;
using Neuron.OpenId.Services.Interfaces;
using OpenIddict.Abstractions;
using Claims = OpenIddict.Abstractions.OpenIddictConstants.Claims;

namespace Neuron.Core.OpenId.Services;

public class CoreIdentityClaimsProvider : IIdentityClaimsProvider
{
    private readonly UserManager<IdpUser> _userManager;

    public CoreIdentityClaimsProvider(UserManager<IdpUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task ProvideClaimsAsync(string userId, ImmutableArray<string> scopes, ClaimsIdentity identity)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return;
        
        var username = await _userManager.GetUserNameAsync(user) ?? "";

        identity.AddClaim(new Claim(Claims.Subject, await _userManager.GetUserIdAsync(user)));
        
        if (scopes.Contains(OpenIddictConstants.Scopes.Profile))
        {
            identity.AddClaim(new Claim(Claims.Name, username));
            identity.AddClaim(new Claim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user) ?? username));
        }
        
        if (scopes.Contains(OpenIddictConstants.Scopes.Email))
            identity.AddClaim(new Claim(Claims.Email, await _userManager.GetEmailAsync(user) ?? ""));
        
        if (scopes.Contains(OpenIddictConstants.Scopes.Roles))
            identity.AddClaims(Claims.Role, [.. await _userManager.GetRolesAsync(user)]);
    }
}