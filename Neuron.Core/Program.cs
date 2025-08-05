using Microsoft.AspNetCore.Http.HttpResults;
using Neuron.Common.Components;
using Neuron.Core.Components;
using Neuron.Core.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAntiforgery();
builder.Services.AddAuthorization();
builder.Services.AddRazorComponents();
builder.AddNeuronCoreIdentity();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};


//app.UseCors();
//app.UseForwardedHeaders();
app.UseSecurityHeaders(policies =>
{
    policies.AddDefaultSecurityHeaders();
    policies.AddPermissionsPolicy(permissions =>
    {
        permissions.AddDefaultSecureDirectives();
        permissions.AddIdentityCredentialsGet().Self();
        permissions.AddPublickeyCredentialsCreate().Self();
        permissions.AddPublickeyCredentialsGet().Self();
    });
});


app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.UseRouting();

app.UseAntiforgery();

app.UseNeuronCoreIdentity();

app.MapGet("/", () => new RazorComponentResult<Home>());
app.MapGet("/test", () => new RazorComponentResult<TestComponent>());

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}