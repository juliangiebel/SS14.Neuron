using Microsoft.AspNetCore.Builder;
using Neuron.Core.OpenId.Endpoints;

namespace Neuron.Core.OpenId;

public static class CoreOpenIdExtension
{
    public static void AddNeuronCoreOpenId(this WebApplicationBuilder builder)
    {
        builder.AddNeuronOpenId();
    }

    public static void UseNeuronCoreOpenId(this WebApplication app)
    {
        app.MapNeuronCoreOpenIdEndpoints();
    }
}