using System.Collections.Immutable;
using System.Security.Claims;

namespace Neuron.OpenId.Types;

public record AuthorizationResult(
    AuthorizationResult.ResultType Type, 
    string AppName,
    string? ErrorName,
    ImmutableArray<string>? Scopes, 
    ClaimsPrincipal? Principal)
{
    public static AuthorizationResult SignIn(ClaimsPrincipal principal) =>
        new(ResultType.SignIn, string.Empty, null, null, principal);

    public static AuthorizationResult Forbidden(string appName, string error) =>
        new(ResultType.Forbidden, appName, error, null, null);

    public static AuthorizationResult Consent(string appName, ImmutableArray<string> scopes) =>
        new(ResultType.Consent, appName, null, scopes, null);
    
    public static AuthorizationResult Error(string appName, string error) =>
        new(ResultType.Error, appName, error, null, null);
    
    public enum ResultType
    {
        SignIn,
        Forbidden,
        Consent,
        Error
    }
}