using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Neuron.Common;

namespace Neuron.Core.ModuleLoader;

public static class ModuleLoaderExtension
{
    public static void AddNeuronCoreModuleLoader(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ModuleLoaderConfiguration>(builder.Configuration.GetSection("ModuleLoader"));
        var configuration = new ModuleLoaderConfiguration();
        builder.Configuration.Bind("ModuleLoader", configuration);

        var basePath = Path.GetFullPath(configuration.ModuleDirectoryPath, Directory.GetCurrentDirectory());
        var assemblies = configuration.Modules
            .Select(x => Path.Combine(basePath, x))
            .Select(Assembly.LoadFrom)
            .Select(x => new AssemblyCatalog(x));

        var catalog = new AggregateCatalog(assemblies);
        
    }   
    
}