using System.ComponentModel.DataAnnotations;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Neuron.Common.Components.Extensions;
using Neuron.Common.Model;
using Neuron.Core.Identity.Components.Account;

namespace Neuron.Core.Identity.Endpoints.Account;

public static class Login2Fa
{
    public static async Task<RazorComponentResult> Get(
        SignInManager<IdpUser> signInManager, 
        bool rememberMe, 
        string? returnUrl = null)
    {
        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
            throw new InvalidOperationException("Unable to load 2FA user.");
        
        return new RazorComponentResult<TwoFactorLoginPage>(new { RememberMe = rememberMe, ReturnUrl = returnUrl });
    }

    public static async Task<Results<RazorComponentResult, RedirectHttpResult, RedirectToRouteHttpResult>> Post(
        HttpContext context,
        SignInManager<IdpUser> signInManager,
        UserManager<IdpUser> userManager,
        [FromForm] Login2FaModel model,
        [FromQuery] bool rememberMe, 
        [FromQuery] string? returnUrl = null)
    {
        returnUrl ??= "~/";
        
        if (context.GetErrors() is not [])
            return new RazorComponentResult<TwoFactorLoginPage>(new { RememberMe = rememberMe, ReturnUrl = returnUrl });
        
        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
            throw new InvalidOperationException("Unable to load 2FA user.");
        
        var code = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(code, rememberMe, model.RememberMachine);
        await userManager.GetUserIdAsync(user);

        if (result.Succeeded)
            return TypedResults.LocalRedirect(returnUrl);

        if (result.IsLockedOut)
            return TypedResults.Redirect("/lockout");
        
        context.AddError("Invalid code.");
        return new RazorComponentResult<TwoFactorLoginPage>(new { RememberMe = rememberMe, ReturnUrl = returnUrl });
    }

    public sealed class Login2FaModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator code")]
        public string TwoFactorCode { get; set; } = null!;
        
        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }
    }
    
    [PublicAPI]
    public sealed class Validator : AbstractValidator<Login2FaModel>
    {
        public Validator()
        {
            RuleFor(model => model.TwoFactorCode).MinimumLength(6);
        }
    }
}