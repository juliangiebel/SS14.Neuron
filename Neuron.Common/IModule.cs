using Microsoft.Extensions.DependencyInjection;

namespace Neuron.Common;

public interface IModule
{
    public void Register(IServiceCollection services);
}