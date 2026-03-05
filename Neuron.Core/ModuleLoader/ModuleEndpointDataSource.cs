using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Primitives;

namespace Neuron.Core.ModuleLoader;

public class ModuleEndpointDataSource : EndpointDataSource
{
    private readonly IServiceProvider _services;
    
    public override IChangeToken GetChangeToken()
    {
        return null;
    }

    public override IReadOnlyList<Endpoint> Endpoints { get; } = new List<Endpoint>();
    
    public void CreateEndpoint()
    {
        var builder = new RouteEndpointBuilder(context => Task.CompletedTask, RoutePatternFactory.Parse("/"), 0)
        {
            Metadata =
            {
                new HttpMethodMetadata([HttpMethod.Get.Method]) 
            }
        };
        
        
        
        var endpoint = builder.Build();

    }
}