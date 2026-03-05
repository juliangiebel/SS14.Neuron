namespace Neuron.Common.Types;

public interface IModuleRegistryHandle
{
    public void RegisterRoute(IModule module, string slug, IRoute route);
}