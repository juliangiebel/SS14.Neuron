using Neuron.Common.Interfaces;

namespace Neuron.Core.ModuleLoader;

public static class ModuleLoaderExtension
{
    private static readonly ILogger Logger = LoggerFactory.Create(builder => builder.AddConsole())
        .CreateLogger("ModuleLoader");
    
    public static void AddNeuronCoreModuleLoader(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ModuleLoaderConfiguration>(builder.Configuration.GetSection("ModuleLoader"));
        builder.Services.AddSingleton<ModuleManager>();
        
        var configuration = new ModuleLoaderConfiguration();
        builder.Configuration.Bind("ModuleLoader", configuration);
        
        var basePath = Path.GetFullPath(configuration.ModuleDirectoryPath, Directory.GetCurrentDirectory());
        
        var assemblies = configuration.Modules
            .Select(x => Path.Combine(basePath, x))
            .Where(FileExists)
            .Select(x => (path: x, assembly: new ModuleLoadContext(x)))
            .Select(x => x.assembly.LoadFromAssemblyPath(x.path))
            .ToList();

        builder.Services.Scan(scan => scan.FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo<IModule>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());
    }   
    
    public static void UseModuleLoader(this WebApplication app)
    {
        app.Services.GetRequiredService<ModuleManager>().LoadModules(app);
    }
    
    private static bool FileExists(string path)
    {
        var exists = File.Exists(path);
        if (!exists)
            Logger.LogWarning("Module file not found: {Path}", path);
        
        return exists;
    }
    
}