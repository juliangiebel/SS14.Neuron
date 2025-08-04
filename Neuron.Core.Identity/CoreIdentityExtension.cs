using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Neuron.Core.Identity.Database;
using OpenIddict.Abstractions;
using OpenIddict.Client;

namespace Neuron.Core.Identity;

public static class CoreIdentityExtension
{
    public static void AddNeuronCoreIdentity(this WebApplicationBuilder builder)
    {
        // TODO: Adapt this further from the example code. Look at the example that passes certification
        builder.Services.AddDbContext<AppIdentityDbContext>(options =>
        {
            options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-velusia-server.sqlite3")}");
            options.UseOpenIddict();
        });
        
        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddOpenIddict()
            .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<AppIdentityDbContext>())
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
                    Issuer = new Uri("changeme", UriKind.Absolute),

                    ClientId = "changeme",
                    ClientSecret = "changeme",
                    RedirectUri = new Uri("changeme", UriKind.Absolute),
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
    }

    public static void UseNeuronCoreIdentity(this WebApplication app)
    {
        
    }
}