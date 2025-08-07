using Microsoft.AspNetCore.Http;

namespace Neuron.Core.OpenId.Endpoints;

public sealed record FormFieldEndpointFilter(string FormFieldName) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.HttpContext.Request.Form.TryGetValue(FormFieldName, out _))
            return next;
        
        return await next(context);
    }
}