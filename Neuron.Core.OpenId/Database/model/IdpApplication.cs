using Neuron.Common.Model;
using OpenIddict.EntityFrameworkCore.Models;
using static Neuron.Core.OpenId.Database.model.OpenIddictDefaultTypes;

namespace Neuron.Core.OpenId.Database.model;

public class IdpApplication : OpenIddictEntityFrameworkCoreApplication<Guid, DefaultAuthorization, DefaultToken>
{
    public Guid? IdpUserId { get; set; }
    public IdpUser? IdpUser { get; set; }
}