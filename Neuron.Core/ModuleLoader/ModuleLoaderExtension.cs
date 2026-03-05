using Microsoft.AspNetCore.Routing;

namespace Neuron.Core.ModuleLoader;

public static class ModuleLoaderExtension
{
    public static void AddNeuronCoreModuleLoader(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ModuleLoaderConfiguration>(builder.Configuration.GetSection("ModuleLoader"));
        builder.Services.AddSingleton<ModuleManager>();
        builder.Services.AddSingleton<ModuleRegistry>();
        builder.Services.AddSingleton<ModuleEndpointDataSource>();
    }   
    
    public static void UseModuleLoader(this WebApplication app)
    {
        var source = app.Services.GetRequiredService<ModuleEndpointDataSource>();
        ((IEndpointRouteBuilder) app).DataSources.Add(source);
        app.Services.GetRequiredService<ModuleManager>().LoadConfiguredModules();
        
    }
    
}