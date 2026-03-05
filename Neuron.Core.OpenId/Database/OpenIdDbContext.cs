using Microsoft.EntityFrameworkCore;
using Neuron.Core.OpenId.Database.model;
using static Neuron.Core.OpenId.Database.model.OpenIddictDefaultTypes;

namespace Neuron.Core.OpenId.Database;

public class OpenIdDbContext(DbContextOptions<OpenIdDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("openid");
        builder.UseOpenIddict<IdpApplication, DefaultAuthorization, DefaultScope, DefaultToken, Guid>();
    }
}