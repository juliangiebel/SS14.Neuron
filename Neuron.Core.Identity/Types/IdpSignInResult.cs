using Microsoft.AspNetCore.Identity;

namespace Neuron.Core.Identity.Types;

public sealed class IdpSignInResult : SignInResult
{
    public bool IsLocked { get; set; }

    public static readonly IdpSignInResult Locked = new() { IsLocked = true };
}