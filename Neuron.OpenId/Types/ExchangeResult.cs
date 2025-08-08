using System.Security.Claims;

namespace Neuron.OpenId.Types;

public record ExchangeResult(ExchangeResult.ResultType Type, string? ErrorName, ClaimsPrincipal? Principal)
{
    public static ExchangeResult Error(string error) => new(ResultType.Error, error, null);
    public static ExchangeResult Forbid(string error) => new(ResultType.Forbid, error, null);
    public static ExchangeResult SignIn(ClaimsPrincipal principal) => new(ResultType.SignIn, null, principal);
    
    public enum ResultType
    {
        Error,
        Forbid,
        SignIn
    }
};