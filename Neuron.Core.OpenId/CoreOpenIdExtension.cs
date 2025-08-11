using System.Reflection;
using System.Text.Json;
using System.Text.Json.Schema;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neuron.Core.OpenId.Configuration;
using Neuron.Core.OpenId.Database;
using Neuron.Core.OpenId.Endpoints;
using Neuron.Core.OpenId.Services;
using Neuron.OpenId;
using Neuron.OpenId.Services.Interfaces;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using OpenIddict.Server;

namespace Neuron.Core.OpenId;

public static class CoreOpenIdExtension
{
    public static void AddNeuronCoreOpenId(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<OpenIdDbContext>(options =>
        {
            options.UseInMemoryDatabase(nameof(OpenIdDbContext));
            options.UseOpenIddict();
        });

        var openId = builder.Services.AddOpenIddict();
        openId.AddCore(options => options.UseEntityFrameworkCore().UseDbContext<OpenIdDbContext>());
        
        openId.AddValidation().UseLocalServer();
        openId.AddValidation().UseAspNetCore();
        
        openId.AddServer().Configure(config => builder.Configuration.Bind("OpenId:Server", config));
        openId.AddServer().UseAspNetCore().EnableAuthorizationEndpointPassthrough().EnableStatusCodePagesIntegration();
        ConfigureCertificates(openId, builder);
        
        builder.Services.AddHostedService<TestDataSeeder>();
        
        builder.AddNeuronOpenId();
        builder.Services.AddScoped<ISignedInIdentityService, CoreSingedInIdentityService>();
        builder.Services.AddScoped<IIdentityClaimsProvider, CoreIdentityClaimsProvider>();
    }

    private static void ConfigureCertificates(OpenIddictBuilder openId, WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            openId.AddServer().AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
            return;
        }

        var config = builder.Configuration
            .GetSection("OpenId")
            .GetSection(OpenIdCertificateConfiguration.Name).Get<OpenIdCertificateConfiguration>();

        if (config?.EncryptionCertificatePath == null || config.SigningCertificatePath == null)
            throw new InvalidOperationException("Encryption and signing certificates not configured");

        using var encryptionCert = File.OpenRead(config.EncryptionCertificatePath);
        openId.AddServer().AddEncryptionCertificate(encryptionCert, config.EncryptionCertificatePassword);
        
        using var signingCert = File.OpenRead(config.SigningCertificatePath);
        openId.AddServer().AddSigningCertificate(signingCert, config.SigningCertificatePassword);
    }

    public static void UseNeuronCoreOpenId(this WebApplication app)
    {
        app.MapNeuronCoreOpenIdEndpoints();
    }
}