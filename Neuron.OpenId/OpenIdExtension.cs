using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Neuron.OpenId.Services;
using Neuron.OpenId.Services.Interfaces;

namespace Neuron.OpenId;

public static class OpenIdExtension
{
    public static void AddNeuronOpenId(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IOpenIdActionService, OpenIdActionService>();
        builder.Services.AddScoped<ApplicationAuthorizationService>();
    }
}