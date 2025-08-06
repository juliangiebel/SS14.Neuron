using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Neuron.Common.Components.Validation;
using Neuron.Common.Validation;
using Neuron.Core.Identity.Components.Account;
using Neuron.Core.Identity.Endpoints.Account;

namespace Neuron.Core.Identity.Endpoints;

public static class Endpoints
{
    public static void MapNeuronCoreIdentityEndpoints(this WebApplication app)
    {
        var account = app.MapGroup("/account")
            .WithTags("Identity UI", "Neuron.Core.Identity");;

        account.MapGet("/login", Login.Get);
        account.MapPost("/login", Login.Post)
            .AddEndpointFilter<UiValidationFilter<Login.LoginModel>>();;
        
        account.MapGet("/login/2fa", () => "")
            .WithName("LoginWith2fa");

        account.MapPost("/login/2fa", Login2Fa.Post)
            .AddEndpointFilter<UiValidationFilter<Login2Fa.Login2FaModel>>();
        
        account.MapGet("/login/recovery", () => "");
        account.MapGet("/login/external", () => "");
        account.MapGet("/logout", () => new RazorComponentResult<LogoutPage>());
        account.MapPost("/logout", Logout.Post);
        
        account.MapGet("/email/confirm", () => Email.ConfirmGet);
        account.MapGet("/email/confirm_change", () => Email.ChangeGet);
        account.MapGet("/email/resend_confirmation", () => "");
        account.MapGet("/password/forgotten", () => "");
        account.MapGet("/password/reset", () => "");
        account.MapGet("/password/reset_confirm", () => new RazorComponentResult<PasswordResetConfirmationPage>());
        
        account.MapGet("/lockout", () => new RazorComponentResult<Lockout>());

        //TODO: translation. That'll also keep strings in code short
        var model = new SimpleMessage.Model(
            "Account locked", 
            "This account has been locked by an administrator. Please contact support if you believe this to be an error."
        )
            ;
        account.MapGet("/locked", () => new RazorComponentResult<SimpleMessage>(model));
        account.MapGet("/access_denied", () => new RazorComponentResult<AccessDenied>());

        var manage = account.MapGroup("/manage")
            .RequireAuthorization();
        
        manage.MapGet("/", () => "");
        manage.MapGet("/change_password", () => "");
        manage.MapGet("/set_password", () => "");
        manage.MapGet("/delete_personal_data", () => "");
        manage.MapGet("/download_personal_data", () => "");
        manage.MapGet("/2fa", () => "");
        manage.MapGet("/disable_2fa", () => "");
        manage.MapGet("/email", () => "");
        manage.MapGet("/enable_authenticator", () => "");
        manage.MapGet("/reset_authenticator", () => "");
        manage.MapGet("/external_logins", () => "");
        manage.MapGet("/generate_recovery_codes", () => "");
        manage.MapGet("/show_recovery_codes", () => "");
        manage.MapGet("/personal_data", () => "");
        manage.MapGet("/set_two_factor_authentication", () => "");
        manage.MapGet("/set_username", () => "");
    }
}