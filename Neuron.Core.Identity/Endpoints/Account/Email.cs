using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Neuron.Core.Identity.Components.Account;
using Neuron.Core.Identity.Model;

namespace Neuron.Core.Identity.Endpoints.Account;

public static class Email
{
    public static async Task<Results<RazorComponentResult, RedirectHttpResult, NotFound>> ConfirmGet(
        UserManager<IdpUser> userManager,
        string? id, 
        string? code)
    {
        if (code is null || id is null)
            return TypedResults.Redirect("/");
        
        var user = await userManager.FindByIdAsync(id);
        
        if (user is null)
            return TypedResults.NotFound();
        
        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await userManager.ConfirmEmailAsync(user, code);

        return new RazorComponentResult<ConfirmEmailPage>(new
        {
            Message = result.Succeeded ? "Email confirmed." : "Error while validating email"
        });
    }

    public static async Task<Results<RazorComponentResult, RedirectHttpResult, NotFound>> ChangeGet(
        UserManager<IdpUser> userManager,
        SignInManager<IdpUser> signInManager,
        string? id,
        string? email,
        string? code)
    {
        if (code is null || id is null || email is null)
            return TypedResults.Redirect("/");
        
        var user = await userManager.FindByIdAsync(id);
        
        if (user is null)
            return TypedResults.NotFound();
        
        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await userManager.ChangeEmailAsync(user, email, code);

        if (!result.Succeeded)
            return new RazorComponentResult<ConfirmEmailPage>(new { Message = "Error while changing email" });
        
        await signInManager.RefreshSignInAsync(user);
        return new RazorComponentResult<ConfirmEmailPage>(new { Message = "Thank you for confirming your email change" });
    }
}