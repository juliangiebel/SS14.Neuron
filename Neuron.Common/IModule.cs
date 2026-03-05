using Microsoft.Extensions.DependencyInjection;
using Neuron.Common.Types;

namespace Neuron.Common;

public interface IModule
{
    public string Name { get; }
    
    public void Register(IModuleRegistryHandle services);
}