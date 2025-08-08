using Microsoft.EntityFrameworkCore;

namespace Neuron.OpenId.Database;

public class OpenIdDbContext(DbContextOptions<OpenIdDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("openid");
    }
}