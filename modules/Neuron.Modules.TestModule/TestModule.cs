using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Neuron.Common.Interfaces;

namespace Neuron.Modules.TestModule;

[PublicAPI]
public class TestModule : IModule
{
    public string Name => "TestModule";

    public void RegisterEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/test", () => new RazorComponentResult<Component1>());
    }
}