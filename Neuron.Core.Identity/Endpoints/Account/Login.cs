using System.ComponentModel.DataAnnotations;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Neuron.Common.Components.Extensions;
using Neuron.Common.Model;
using Neuron.Core.Identity.Components.Account;
using Neuron.Core.Identity.Extensions;
using Neuron.Core.Identity.Types;

namespace Neuron.Core.Identity.Endpoints.Account;

public static class Login
{
    public static async Task<RazorComponentResult> Get(
        HttpContext context, 
        [FromServices] SignInManager<IdpUser> signInManager,
        [FromServices] IAntiforgery antiforgery,
        [FromQuery] string? returnUrl = null)
    {
        returnUrl ??= "~/";

        await context.SignOutAsync(IdentityConstants.ExternalScheme);
        return await LoginPageResult(signInManager, returnUrl, antiforgery.GetAndStoreTokens(context));
    }

    public static async Task<Results<RazorComponentResult, RedirectHttpResult, RedirectToRouteHttpResult>> Post(
        HttpContext context, 
        [FromServices] SignInManager<IdpUser> signInManager,
        [FromServices] IAntiforgery antiforgery,
        [FromServices] ILogger<LoginPage> logger,
        [FromForm] LoginModel model,
        [FromQuery] string? returnUrl = null)
    {
        returnUrl ??= "~/";

        if (context.GetErrors() is not [])
            return await LoginPageResult(signInManager, returnUrl, antiforgery.GetAndStoreTokens(context));
        
        var user = await signInManager.UserManager.FindByNameOrEmailAsync(model.Email);
        if (user is null)
        {
            context.AddError("Invalid login attempt.");
            return await LoginPageResult(signInManager, returnUrl, antiforgery.GetAndStoreTokens(context));
        }
        
        var emailConfirmed = await signInManager.UserManager.IsEmailConfirmedAsync(user);
        if (signInManager.UserManager.Options.SignIn.RequireConfirmedEmail && !emailConfirmed)
        {
            context.AddError("The email address for this account still needs to be confirmed. " +
                             "Please confirm your email address before trying to log in.");
            
            return await LoginPageResult(signInManager, returnUrl, antiforgery.GetAndStoreTokens(context));
        }
        
        var result = await signInManager.PasswordSignInAsync(user?.UserName ?? model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        
        if (result.Succeeded)
        {
            logger.LogDebug("User logged in.");
            return TypedResults.LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor)
            return TypedResults.RedirectToRoute("LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

        if (result.IsLockedOut)
        {
            logger.LogWarning("User locked out.");
            return TypedResults.Redirect("/lockout");
        }

        if (result is IdpSignInResult {IsLocked: true})
        {
            logger.LogWarning("User locked by administrator.");
            return TypedResults.Redirect("/locked");
        }
        
        context.AddError("Invalid login attempt.");
        return await LoginPageResult(signInManager, returnUrl, antiforgery.GetAndStoreTokens(context));
    }

    private static async Task<RazorComponentResult> LoginPageResult(SignInManager<IdpUser> manager, string returnUrl, AntiforgeryTokenSet token)
    {
        var externalLogins = (await manager.GetExternalAuthenticationSchemesAsync()).ToList();
        
        return new RazorComponentResult<LoginPage>(new
        {
            ReturnUrl = returnUrl, 
            ExternalLogins = externalLogins,
            Token = token
        });
    }
    
    public sealed class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    [PublicAPI]
    public sealed class Validator : AbstractValidator<LoginModel>
    {
        public Validator()
        {
            RuleFor(model => model.Email).EmailAddress();
        }
    }
}