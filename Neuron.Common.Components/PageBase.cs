using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Neuron.Common.Components;

public abstract class PageBase : ComponentBase
{
    [CascadingParameter]
    public HttpContext? Context { get; set; }
}