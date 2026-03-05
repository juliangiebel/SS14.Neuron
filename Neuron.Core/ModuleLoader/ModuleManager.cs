using System.Reflection;
using Microsoft.Extensions.Options;
using Neuron.Common;

namespace Neuron.Core.ModuleLoader;

public class ModuleManager
{
    private readonly ILogger<ModuleManager> _logger;
    private readonly IOptionsMonitor<ModuleLoaderConfiguration> _configMonitor;
    private readonly ModuleRegistry _registry;

    public ModuleManager(IOptionsMonitor<ModuleLoaderConfiguration> configuration, ILogger<ModuleManager> logger, ModuleRegistry registry)
    {
        _configMonitor = configuration;
        _logger = logger;
        _registry = registry;
    }
    
    public void LoadConfiguredModules()
    {
        var configuration = _configMonitor.CurrentValue;
        
        var basePath = Path.GetFullPath(configuration.ModuleDirectoryPath, Directory.GetCurrentDirectory());
        
        var assemblies = configuration.Modules
            .Select(x => Path.Combine(basePath, x))
            .Where(FileExists)
            .Select(x => (path: x, assembly: new ModuleLoadContext(x)))
            .Select(x => x.assembly.LoadFromAssemblyPath(x.path))
            .ToList();
        
        var modules = assemblies.SelectMany(x => x.GetTypes())
            .Where(t => typeof(IModule).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
            .Select(Activator.CreateInstance)
            .Cast<IModule>()
            .ToList();
        
        _registry.Register(modules);
    }
    
    private bool FileExists(string path)
    {
        var exists = File.Exists(path);
        if (!exists)
            _logger.LogWarning("Module file not found: {Path}", path);
        
        return exists;
    }
}