using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Neuron.Core.OpenId.Endpoints;

public static class Endpoints
{
    public static void MapNeuronCoreOpenIdEndpoints(this WebApplication app)
    {
        var authorize = app.MapGroup("/connect/authorize")
            .WithTags("OpenId UI", "Neuron.Core.OpenId");
        
        authorize.MapGet("/", Authorization.Authorize.GetAndPost);
        authorize.MapPost("/", Authorization.Authorize.GetAndPost);
        
        authorize.MapPost("/accept", Authorization.Accept.Post)
            .RequireAuthorization();

        authorize.MapPost("/deny", () => AuthResults.Forbid("access_denied", "Access denied"))
            .RequireAuthorization();

        authorize.MapGet("/logout", Authorization.Logout.Get);
        authorize.MapPost("/logout", Authorization.Logout.Post);
        
        //app.MapPost("/connect/token", Authorization.ExchangeToken.Post)
        //    .WithTags("OpenId API", "Neuron.Core.OpenId");


        app.MapPost("/debug/token", Debug.TestToken.Post)
            .DisableAntiforgery();
    }

}