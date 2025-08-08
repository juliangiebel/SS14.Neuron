using Microsoft.AspNetCore.Builder;
using Neuron.Core.OpenId.Endpoints;
using Neuron.OpenId;

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