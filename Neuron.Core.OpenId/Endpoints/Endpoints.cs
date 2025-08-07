using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Neuron.Core.OpenId.Endpoints;

public static class Endpoints
{
    public static void MapNeuronCoreOpenIdEndpoints(this WebApplication app)
    {
        var account = app.MapGroup("/connect/authorize")
            .WithTags("OpenId UI", "Neuron.Core.OpenId");;
        
        account.MapGet("/", Authorization.Authorize.GetAndPost);
        account.MapPost("/", Authorization.Authorize.GetAndPost);
        
        account.MapPost("/accept", Authorization.Accept.Post)
            .RequireAuthorization();
        
    }

}