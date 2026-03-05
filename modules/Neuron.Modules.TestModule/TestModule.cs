using Microsoft.Extensions.DependencyInjection;
using Neuron.Common;
using Neuron.Common.Types;

namespace Neuron.Modules.TestModule;

public class TestModule : IModule
{
    public string Name => "TestModule";
    public void Register(IModuleRegistryHandle registryHandle)
    {
        registryHandle.RegisterRoute(this, "test", new TestRoute());
    }
}