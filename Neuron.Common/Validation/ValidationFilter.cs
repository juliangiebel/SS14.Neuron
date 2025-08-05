using FluentValidation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Neuron.Common.Validation;

[PublicAPI]
public sealed class ValidationFilter<TRequest> (IValidator<TRequest> validator): IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.SingleOrDefault(argument => argument?.GetType() == typeof(TRequest));
        
        if (argument is not TRequest request)
            throw new ArgumentException($"Could not find argument of type {typeof(TRequest)}");
        
        var result = await validator.ValidateAsync(request);

        if (!result.IsValid)
            return Results.BadRequest(result.Errors.Select(error => error.ErrorMessage));
        
        return await next(context);
    }
}