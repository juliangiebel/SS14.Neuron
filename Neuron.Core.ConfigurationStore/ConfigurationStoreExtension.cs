using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Neuron.Core.ConfigurationStore.Database;
using Neuron.Core.ConfigurationStore.Services;
using SS14.ConfigProvider;

namespace Neuron.Core.ConfigurationStore;

public static class ConfigurationStoreExtension
{
    public static void AddConfigurationStore(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ConfigurationStoreDbContext>(options =>
        {
            options.UseInMemoryDatabase(nameof(ConfigurationStoreDbContext));
        });
        
        builder.Configuration.AddConfigurationDb<ConfigurationStoreDbContext>(b =>
        {
            b.UseInMemoryDatabase(nameof(ConfigurationStoreDbContext));
        });
        
        builder.Services.AddSingleton<ConfigurationStoreService>();
    }
}