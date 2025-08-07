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
        
        await PopulateRolesAsync(scope);
        await PopulateUsersAsync(scope);
        await AssignRolesAsync(scope);
    }

    private async ValueTask AssignRolesAsync(IServiceScope scope)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdpUser>>();
        
        var user = await userManager.FindByNameAsync("TestUser");
        if (user == null) 
            return;

        var roles = new[] { "Admin", "User", "Moderator" };
        foreach (var role in roles)
        {
            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
        }
    }

    private async ValueTask PopulateRolesAsync(IServiceScope scope)
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdpRole>>();
        
        var roles = new[]
        {
            "Admin",
            "User",
            "Moderator"
        };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdpRole { Name = roleName });
            }
        }
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