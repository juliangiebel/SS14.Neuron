using Microsoft.AspNetCore.Identity;
using Neuron.Common.Model;

namespace Neuron.Core.Identity.Extensions;

public static class SignInManagerExtension
{
    public static async Task<IdpUser?> FindByNameOrEmailAsync(this UserManager<IdpUser> manager, string nameOrEmail)
    {
        var user = await manager.FindByEmailAsync(nameOrEmail);

        if (user is not null)
            return user;
        
        return await manager.FindByNameAsync(nameOrEmail);
    }
}