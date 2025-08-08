﻿using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;

namespace Neuron.OpenId.Types;

public record AuthenticationValidationFailure(
    string? Error = null,
    bool IsChallenge = false,
    AuthenticationProperties? Properties = null)
{
    public static AuthenticationValidationFailure LoginRequired => new(Error: OpenIddictConstants.Errors.LoginRequired);
        
    public static AuthenticationValidationFailure Challenge (AuthenticationProperties properties) =>
        new(IsChallenge: true, Properties: properties);
};
