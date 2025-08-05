using System.ComponentModel.DataAnnotations;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Neuron.Core.Identity.Components;
using Neuron.Core.Identity.Model;

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
        var externalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        
        return new RazorComponentResult<LoginPage>(new
        {
            ReturnUrl = returnUrl,
            ExternalLogins = externalLogins,
            Token = antiforgery.GetAndStoreTokens(context),
        });
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

        var test = await signInManager.UserManager.Users.ToListAsync();
        
        var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
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
        
        var externalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        
        return new RazorComponentResult<LoginPage>(new
        {
            ReturnUrl = returnUrl, 
            ExternalLogins = externalLogins,
            Token = antiforgery.GetAndStoreTokens(context),
            Error = "Invalid login attempt."
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