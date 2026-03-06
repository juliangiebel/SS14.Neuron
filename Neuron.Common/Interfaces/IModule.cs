using Microsoft.AspNetCore.Routing;

namespace Neuron.Common.Interfaces;

public interface IModule
{
    public string Name { get; }

    public string? Slug => null;

    public void RegisterEndpoints(IEndpointRouteBuilder endpoints);
}