using Microsoft.AspNetCore.Http;

namespace Neuron.Common.Components.Extensions;

public static class Toast
{
    private const string ToastKey = "toasts";
    
    public static void AddToast(this HttpContext context, Type type, string message)
    {
        if (!context.Items.ContainsKey(ToastKey))
            context.Items[ToastKey] = new List<Content>();

        if (context.Items[ToastKey] is not List<Content> errors)
            return;
        
        errors.Add(new Content(type, message));
    }
    
    public static List<Content> GetToasts(this HttpContext context) => context.Items[ToastKey] as List<Content> ?? [];

    public record Content(Type Type, string Message);
    
    public enum Type
    {
        Success,
        Error,
        Info,
        Warning,
    }
}