using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Forge4BlazorRCL
{
    public class ForgeViewerJsInterop
    {

        public static async Task StartViewer(IJSRuntime jsRuntime, string aToken, string aLocation)
        {
            var stuff = await jsRuntime.InvokeAsync<string>("forgeViewerJsFunctions.startViewer", new object[] { aToken, aLocation });
        }

        public static async Task<string> LoadDocument(IJSRuntime jsRuntime, string aUrn)
        {
            var aResult = await jsRuntime.InvokeAsync<string>("forgeViewerJsFunctions.loadDocument", new object[] { aUrn });
            return aResult;
        }
        public static async Task LoadNode(IJSRuntime jsRuntime, string aViewable)
        {
            await jsRuntime.InvokeAsync<string>("forgeViewerJsFunctions.loadDocumentNode", new object[] { aViewable });
        }
        public static async Task LoadFile(IJSRuntime jsRuntime, string aUri)
        {
            await jsRuntime.InvokeAsync<string>("forgeViewerJsFunctions.loadFile", new object[] { aUri });
        }

        public static async Task LoadExtension(IJSRuntime jsRuntime, string aExtension)
        {
            await jsRuntime.InvokeAsync<string>("forgeViewerJsFunctions.loadExtension", new object[] { aExtension });
        }
    }
}
