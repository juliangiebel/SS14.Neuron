using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neuron.Common.Model;
using Neuron.Core.Identity.Types;

namespace Neuron.Core.Identity.Services;

public sealed class IdpSignInManager : SignInManager<IdpUser>
{
    public IdpSignInManager(
        UserManager<IdpUser> userManager, 
        IHttpContextAccessor contextAccessor, 
        IUserClaimsPrincipalFactory<IdpUser> claimsFactory, 
        IOptions<IdentityOptions> optionsAccessor, 
        ILogger<SignInManager<IdpUser>> logger, 
        IAuthenticationSchemeProvider schemes, 
        IUserConfirmation<IdpUser> confirmation
    ) : base(
        userManager, 
        contextAccessor, 
        claimsFactory, 
        optionsAccessor, 
        logger, 
        schemes, 
        confirmation)
    {
    }

    protected override async Task<SignInResult?> PreSignInCheck(IdpUser user)
    {
        if (user.Locked)
            return IdpSignInResult.Locked;
        
        return await base.PreSignInCheck(user);
    }
}