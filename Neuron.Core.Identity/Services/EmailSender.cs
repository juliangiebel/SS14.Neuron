using Microsoft.AspNetCore.Identity;
using Neuron.Core.Identity.Model;

namespace Neuron.Core.Identity.Services;

public class EmailSender : IEmailSender<IdpUser>
{
    public Task SendConfirmationLinkAsync(IdpUser user, string email, string confirmationLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetLinkAsync(IdpUser user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetCodeAsync(IdpUser user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }
}