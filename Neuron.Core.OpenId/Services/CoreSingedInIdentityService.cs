using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Neuron.Common.Model;
using Neuron.OpenId.Services.Interfaces;
using OpenIddict.Abstractions;

namespace Neuron.Core.OpenId.Services;

public class CoreSingedInIdentityService : ISignedInIdentityService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<IdpUser> _userManager;
    private readonly SignInManager<IdpUser> _signInManager;

    public CoreSingedInIdentityService(IHttpContextAccessor httpContextAccessor, UserManager<IdpUser> userManager, SignInManager<IdpUser> signInManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<bool> IsAvailableAsync()
    {
        if (_httpContextAccessor.HttpContext is not { } context)
            return false;

        var result = await context.AuthenticateAsync();
        return result.Succeeded;
    }

    public async Task<string?> GetUserIdAsync()
    {
        if (_httpContextAccessor.HttpContext is not { } context)
            return null;
        
        var user = await _userManager.GetUserAsync(context.User);
        if (user is null)
            return null;
        
        return await _userManager.GetUserIdAsync(user);
    }

    public async Task<bool> CanSignInAsync()
    {
        if (_httpContextAccessor.HttpContext is not { } context)
            return false;
        
        var user = await _userManager.GetUserAsync(context.User);
        return user is not null && await _signInManager.CanSignInAsync(user);
    }

    public async Task<bool> IsAvailableAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId) is not null;
    }

    public async Task<string?> GetUserIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return null;
        
        return await _userManager.GetUserIdAsync(user);
    }

    public async Task<bool> CanSignInAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is not null && await _signInManager.CanSignInAsync(user);
    }
}