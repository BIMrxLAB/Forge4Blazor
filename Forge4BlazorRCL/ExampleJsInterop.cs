using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Forge4BlazorRCL
{
    public class ExampleJsInterop
    {
        public static ValueTask<string> Promptt(IJSRuntime jsRuntime, string message)
        {
            // Implemented in exampleJsInterop.js
            return jsRuntime.InvokeAsync<string>(
                "exampleJsFunctions.showPrompt",
                message);
        }
    }
}
