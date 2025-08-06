using FluentValidation;
using Microsoft.AspNetCore.Http;
using Neuron.Common.Components.Extensions;

namespace Neuron.Common.Components.Validation;

public sealed class UiValidationFilter<TRequest> (IValidator<TRequest> validator): IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.SingleOrDefault(argument => argument?.GetType() == typeof(TRequest));
        
        if (argument is not TRequest request)
            throw new ArgumentException($"Could not find argument of type {typeof(TRequest)}");
        
        var result = await validator.ValidateAsync(request);

        if (result.IsValid) return await next(context);
        
        foreach (var error in result.Errors)
        {
            if(error is null)
                continue;
            
            context.HttpContext.AddError(error.ErrorMessage);
        }

        return await next(context);
    }
}