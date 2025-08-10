using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Neuron.OpenId.Services;
using Neuron.OpenId.Services.Interfaces;
using OpenIddict.Abstractions;
using OpenIddict.Client;

namespace Neuron.OpenId;

public static class OpenIdExtension
{
    public static void AddNeuronOpenId(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IOpenIdActionService, OpenIdActionService>();
        builder.Services.AddScoped<ApplicationAuthorizationService>();
    }
}