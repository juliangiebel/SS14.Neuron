using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation;

namespace Neuron.Core.OpenId.Endpoints.Debug;

public static class TestToken
{
    public static async Task<string> Post(
        [FromServices] OpenIddictValidationService validator,
        [FromForm] string token)
    {
        var principal = await validator.ValidateAccessTokenAsync(token);
        return JsonSerializer.Serialize(principal.Claims.Select(c => $"{c.Type}: {c.Value}").ToArray());
    }
}