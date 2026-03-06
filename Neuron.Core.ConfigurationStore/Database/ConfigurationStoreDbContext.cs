using Microsoft.EntityFrameworkCore;
using SS14.ConfigProvider.Model;
using ConfigStore = SS14.ConfigProvider.Model.ConfigurationStore;

namespace Neuron.Core.ConfigurationStore.Database;

public class ConfigurationStoreDbContext : DbContext, IConfigDbContext
{
    public DbSet<ConfigStore> ConfigurationStore { get; set; }
    
    public ConfigurationStoreDbContext(DbContextOptions<ConfigurationStoreDbContext> options) : base(options) {} 
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("config");
        base.OnModelCreating(builder);
    }
}