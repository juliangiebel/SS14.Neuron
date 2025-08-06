using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Neuron.Core.OpenId.Database;
using OpenIddict.Abstractions;
using OpenIddict.Client;

namespace Neuron.Core.OpenId;

public static class OpenIdExtension
{
    public static void AddNeuronOpenId(this WebApplicationBuilder builder)
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
                options.SetAuthorizationEndpointUris("connect/authorize")
                    .SetTokenEndpointUris("connect/token")
                    .SetEndSessionEndpointUris("connect/endsession")
                    .SetUserInfoEndpointUris("connect/userinfo");

                options.RegisterScopes(OpenIddictConstants.Scopes.Email);

                options.AllowAuthorizationCodeFlow();

                // Register the signing and encryption credentials.
                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableStatusCodePagesIntegration();
                
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        
        builder.Services.AddHostedService<TestDataSeeder>();
    }
}