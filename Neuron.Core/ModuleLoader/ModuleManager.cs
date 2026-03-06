using System.Globalization;
using Microsoft.Extensions.Options;
using Neuron.Common.Extensions;
using Neuron.Common.Interfaces;

namespace Neuron.Core.ModuleLoader;

public class ModuleManager
{
    private readonly ILogger<ModuleManager> _logger;
    private readonly IOptionsMonitor<ModuleLoaderConfiguration> _configMonitor;
    private readonly IEnumerable<IModule> _modules;

    public ModuleManager(IOptionsMonitor<ModuleLoaderConfiguration> configuration, ILogger<ModuleManager> logger, IEnumerable<IModule> modules)
    {
        _configMonitor = configuration;
        _logger = logger;
        _modules = modules;
    }
    
    public void LoadModules(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/modules");
        
        foreach (var module in _modules)
        {
            var slug = module.Slug ?? module.Name.ToSnakeCase();
            if (!Uri.IsWellFormedUriString(slug, UriKind.Relative))
            {
                _logger.LogError("Module {Module} has invalid slug {Slug}", module.Name, slug);
                continue;
            }
            
            var subgroup = group.MapGroup(slug);
            module.RegisterEndpoints(subgroup);
        }
        
        
        /*var configuration = _configMonitor.CurrentValue;
        
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
        
        _registry.Register(modules);*/
    }
}