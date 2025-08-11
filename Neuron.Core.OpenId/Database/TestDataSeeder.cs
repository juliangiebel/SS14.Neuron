using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

namespace Neuron.Core.OpenId.Database;

public sealed class TestDataSeeder(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        await PopulateScopes(scope, ct);
        await PopulateInternalApps(scope, ct);
    }
    
    public Task StopAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
    
    private async ValueTask PopulateScopes(IServiceScope scope, CancellationToken ct)
    {
        var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var appDescriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "test_client",
            ClientSecret = "test_secret",
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            DisplayName = "Test Client",
            RedirectUris = { new Uri("https://localhost:7175/callback") },
            Requirements = { OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.Introspection,
                OpenIddictConstants.Permissions.Endpoints.EndSession,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Roles,
                OpenIddictConstants.Permissions.Prefixes.Scope + "test_scope"
            }
        };
        
        var client = await appManager.FindByClientIdAsync(appDescriptor.ClientId, ct);
        if (client == null)
        {
            await appManager.CreateAsync(appDescriptor, ct);
        }
        else
        {
            await appManager.UpdateAsync(client, appDescriptor, ct);
        }
    }
    
    private async ValueTask PopulateInternalApps(IServiceScope scope, CancellationToken ct)
    {
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
        var scopeDescriptor = new OpenIddictScopeDescriptor
        {
            Name = "test_scope",
            Resources = { "test_resource" }
        };
        
        var scopeEntity = await scopeManager.FindByNameAsync(scopeDescriptor.Name, ct);
        if (scopeEntity == null)
        {
            await scopeManager.CreateAsync(scopeDescriptor, ct);
        }
        else
        {
            await scopeManager.UpdateAsync(scopeEntity, scopeDescriptor, ct);       
        }
    }
}