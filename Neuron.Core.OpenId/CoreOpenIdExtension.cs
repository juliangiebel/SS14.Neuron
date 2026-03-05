using System.Runtime.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neuron.Core.OpenId.Configuration;
using Neuron.Core.OpenId.Database;
using Neuron.Core.OpenId.Database.model;
using Neuron.Core.OpenId.Endpoints;
using Neuron.Core.OpenId.Services;
using Neuron.Core.OpenId.Services.Interfaces;
using static Neuron.Core.OpenId.Database.model.OpenIddictDefaultTypes;

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
        openId.AddCore(options =>
        {
            options.UseEntityFrameworkCore().UseDbContext<OpenIdDbContext>()
                .ReplaceDefaultEntities<IdpApplication, DefaultAuthorization, DefaultScope,  DefaultToken, Guid>();
        });
        
        openId.AddValidation().UseLocalServer();
        openId.AddValidation().UseAspNetCore();
        
        openId.AddServer().Configure(config => builder.Configuration.Bind("OpenId:Server", config));
        openId.AddServer().UseAspNetCore().EnableAuthorizationEndpointPassthrough().EnableStatusCodePagesIntegration();
        ConfigureCertificates(openId, builder);
        
        builder.Services.AddHostedService<TestDataSeeder>();
        builder.Services.AddScoped<ISignedInIdentityService, CoreSingedInIdentityService>();
        builder.Services.AddScoped<IIdentityClaimsProvider, CoreIdentityClaimsProvider>();
        builder.Services.AddScoped<IOpenIdActionService, OpenIdActionService>();
        builder.Services.AddScoped<ApplicationAuthorizationService>();
    }

    private static void ConfigureCertificates(OpenIddictBuilder openId, WebApplicationBuilder builder)
    {
        /*if (builder.Environment.IsDevelopment())
        {
            openId.AddServer().AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
            return;
        }*/

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

    [UnsupportedOSPlatform("browser")]
    public static void UseNeuronCoreOpenId(this WebApplication app)
    {
        app.MapNeuronCoreOpenIdEndpoints();
    }
}