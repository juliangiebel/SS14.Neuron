using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Neuron.Common.Model;
using Neuron.Core.Identity.Database;
using Neuron.Core.Identity.Endpoints;
using Neuron.Core.Identity.Services;

namespace Neuron.Core.Identity;

public static class CoreIdentityExtension
{
    public static void AddNeuronCoreIdentity(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppIdentityDbContext>(options =>
        {
            options.UseInMemoryDatabase(nameof(AppIdentityDbContext));
        });
        
        builder.Services.AddIdentity<IdpUser, IdpRole>()
            .AddSignInManager<IdpSignInManager>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton<IEmailSender<IdpUser>, EmailSender>();
        builder.Services.AddHostedService<TestDataSeeder>();

        builder.Services.AddValidatorsFromAssemblyContaining<AppIdentityDbContext>();
    }

    public static void UseNeuronCoreIdentity(this WebApplication app)
    {
        app.MapGroup("/api/identity")
            .MapIdentityApi<IdpUser>()
            .WithTags("Identity API", "Neuron.Core.Identity");
        
        app.MapNeuronCoreIdentityEndpoints();
    }
}