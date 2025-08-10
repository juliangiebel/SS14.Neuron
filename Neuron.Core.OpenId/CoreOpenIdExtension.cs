using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Neuron.Core.OpenId.Database;
using Neuron.Core.OpenId.Endpoints;
using Neuron.Core.OpenId.Services;
using Neuron.OpenId;
using Neuron.OpenId.Services.Interfaces;
using OpenIddict.Abstractions;
using OpenIddict.Client;

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
        
        builder.Services.AddOpenIddict()
            .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<OpenIdDbContext>())
            .AddClient(options =>
            {
                options.AllowAuthorizationCodeFlow();

                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                    .EnableStatusCodePagesIntegration()
                    .EnableRedirectionEndpointPassthrough();

                options.UseSystemNetHttp().SetProductInformation(Assembly.GetEntryAssembly()!);

                options.AddRegistration(new OpenIddictClientRegistration
                {
                    Issuer = new Uri("http://changeme.test", UriKind.Absolute),

                    ClientId = "changeme",
                    ClientSecret = "changeme",
                    RedirectUri = new Uri("changeme", UriKind.Relative),
                });
            })
            .AddServer(options =>
            {
                options.SetAuthorizationEndpointUris("connect/authorize", "connect/authorize/accept",
                        "connect/authorize/deny")
                    .SetTokenEndpointUris("connect/token")
                    .SetEndSessionEndpointUris("connect/endsession")
                    .SetUserInfoEndpointUris("connect/userinfo")
                    .SetIntrospectionEndpointUris("connect/introspect");

                options.AllowAuthorizationCodeFlow();
                options.AllowRefreshTokenFlow();
                options.RegisterScopes(
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Roles,
                    OpenIddictConstants.Scopes.OfflineAccess
                );

                // Register the signing and encryption credentials.
                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();
                
                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    //.EnableEndSessionEndpointPassthrough()
                    //.EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableStatusCodePagesIntegration();
                
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        builder.Services.AddHostedService<TestDataSeeder>();
        
        builder.AddNeuronOpenId();
        builder.Services.AddScoped<ISignedInIdentityService, CoreSingedInIdentityService>();
        builder.Services.AddScoped<IIdentityClaimsProvider, CoreIdentityClaimsProvider>();
    }

    public static void UseNeuronCoreOpenId(this WebApplication app)
    {
        app.MapNeuronCoreOpenIdEndpoints();
    }
}