using Microsoft.AspNetCore.Http;

namespace Neuron.Common.Components.Extensions;

public static class HttpContextErrorExtension
{
    private const string ErrorKey = "errors";
    
    public static void AddError(this HttpContext context, string error)
    {
        if (!context.Items.ContainsKey(ErrorKey))
            context.Items[ErrorKey] = new List<string>();

        if (context.Items[ErrorKey] is not List<string> errors)
            return;
        
        errors.Add(error);
    }
    
    public static List<string> GetErrors(this HttpContext context) => context.Items[ErrorKey] as List<string> ?? [];
}