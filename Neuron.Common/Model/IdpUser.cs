using Microsoft.AspNetCore.Identity;

namespace Neuron.Common.Model;

public class IdpUser : IdentityUser<Guid>
{
    /// <summary>
    /// Account has been locked and cannot be logged into anymore.
    /// </summary>
    public bool Locked { get; set; }
}