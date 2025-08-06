using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neuron.Common.Model;

namespace Neuron.Core.Identity.Database;

public sealed class TestDataSeeder(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        
        await PopulateUsersAsync(scope);
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
    private async ValueTask PopulateUsersAsync(IServiceScope scope)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdpUser>>();

        var user = new IdpUser
        {
            UserName = "TestUser",
            Email = "test@example.test"
        };
        
        await userManager.CreateAsync(user, "Test123456$");
    }
}