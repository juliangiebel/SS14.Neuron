using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Neuron.Core.OpenId.Endpoints;

public static class Endpoints
{
    public static void MapNeuronCoreOpenIdEndpoints(this WebApplication app)
    {
        var account = app.MapGroup("/connect/consent")
            .WithTags("OpenId UI", "Neuron.Core.OpenId");;
        
        account.MapGet("/", Consent.Get);
        
    }

}