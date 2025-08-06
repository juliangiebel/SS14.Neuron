using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Neuron.Common.Model;

namespace Neuron.Core.Identity.Database;

public class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : IdentityDbContext<IdpUser, IdpRole, Guid>
    (options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("identity");
        
        base.OnModelCreating(builder);
    }
}