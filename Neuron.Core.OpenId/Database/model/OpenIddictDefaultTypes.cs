using OpenIddict.EntityFrameworkCore.Models;

namespace Neuron.Core.OpenId.Database.model;

public class OpenIddictDefaultTypes
{
    public sealed class DefaultAuthorization : OpenIddictEntityFrameworkCoreAuthorization<Guid, IdpApplication, DefaultToken>;
    public sealed class DefaultScope : OpenIddictEntityFrameworkCoreScope<Guid>;
    public sealed class DefaultToken : OpenIddictEntityFrameworkCoreToken<Guid, IdpApplication, DefaultAuthorization>;
}