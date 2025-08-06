using System.Security.Claims;

namespace Neuron.Common.Components.Extensions;

public static class ClaimsPrincipalExtension
{
    public static string? DisplayName(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Name);
}