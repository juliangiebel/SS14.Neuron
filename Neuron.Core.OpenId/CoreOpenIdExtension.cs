using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Neuron.Core.OpenId.Endpoints;
using Neuron.Core.OpenId.Services;
using Neuron.OpenId;
using Neuron.OpenId.Services.Interfaces;

namespace Neuron.Core.OpenId;

public static class CoreOpenIdExtension
{
    public static void AddNeuronCoreOpenId(this WebApplicationBuilder builder)
    {
        builder.AddNeuronOpenId();
        builder.Services.AddScoped<ISignedInIdentityService, CoreSingedInIdentityService>();
        builder.Services.AddScoped<IIdentityClaimsProvider, CoreIdentityClaimsProvider>();
    }

    public static void UseNeuronCoreOpenId(this WebApplication app)
    {
        app.MapNeuronCoreOpenIdEndpoints();
    }
}