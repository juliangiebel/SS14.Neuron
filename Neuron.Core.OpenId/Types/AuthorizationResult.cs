using System.Collections.Immutable;
using System.Security.Claims;
using Neuron.Core.OpenId.Database.model;

namespace Neuron.Core.OpenId.Types;

public record AuthorizationResult(
    AuthorizationResult.ResultType Type, 
    IdpApplication? Application,
    string? ErrorName,
    ImmutableArray<string>? Scopes, 
    ClaimsPrincipal? Principal)
{
    public static AuthorizationResult SignIn(ClaimsPrincipal principal) =>
        new(ResultType.SignIn, null, null, null, principal);

    public static AuthorizationResult Forbidden(IdpApplication app, string error) =>
        new(ResultType.Forbidden, app, error, null, null);

    public static AuthorizationResult Consent(IdpApplication app, ImmutableArray<string> scopes) =>
        new(ResultType.Consent, app, null, scopes, null);
    
    public static AuthorizationResult Error(IdpApplication app, string error) =>
        new(ResultType.Error, app, error, null, null);
    
    public enum ResultType
    {
        SignIn,
        Forbidden,
        Consent,
        Error
    }
}