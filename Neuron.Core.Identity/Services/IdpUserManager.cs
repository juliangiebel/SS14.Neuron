using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neuron.Common.Model;
using Neuron.Core.Identity.Database;

namespace Neuron.Core.Identity.Services;

public sealed class IdpUserManager : UserManager<IdpUser>
{
    private readonly AppIdentityDbContext _dbContext;
    private readonly ISystemClock _clock;
    
    public IdpUserManager(
        IUserStore<IdpUser> store, 
        IOptions<IdentityOptions> optionsAccessor, 
        IPasswordHasher<IdpUser> passwordHasher, 
        IEnumerable<IUserValidator<IdpUser>> userValidators, 
        IEnumerable<IPasswordValidator<IdpUser>> passwordValidators, 
        ILookupNormalizer keyNormalizer, 
        IdentityErrorDescriber errors, 
        IServiceProvider services, 
        ILogger<UserManager<IdpUser>> logger, 
        AppIdentityDbContext dbContext, 
        ISystemClock clock) : base(
        store, 
        optionsAccessor, 
        passwordHasher, 
        userValidators, 
        passwordValidators, 
        keyNormalizer, 
        errors,
        services, 
        logger)
    {
        _dbContext = dbContext;
        _clock = clock;
    }
    
    public override async Task<IdentityResult> CreateAsync(IdpUser user, string password)
    {
        var result = await base.CreateAsync(user, password);
        if (!result.Succeeded)
            return result;

        // TODO: log account actions
        
        return result;
    }
    
}