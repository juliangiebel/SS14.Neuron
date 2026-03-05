using Neuron.Common;
using Neuron.Common.Types;

namespace Neuron.Core.ModuleLoader;

public class ModuleRegistry : IModuleRegistryHandle
{
    private readonly HashSet<IModule> _modules = [];
    private readonly Dictionary<string, IRoute> _routes = new();
    
    public void Register(ICollection<IModule> modules)
    {
        foreach (var module in modules)
        {
            if (_modules.Add(module))
                module.Register(this);
        }
    }

    public void Register(IModule module)
    {
        if (_modules.Add(module))
            module.Register(this);   
    }

    public void RegisterRoute(IModule module,  string slug, IRoute route)
    {
        if (!_routes.TryAdd($"{module.Name}_{slug}", route))
            throw new ArgumentException("Route with the same slug already exists");
    }
}