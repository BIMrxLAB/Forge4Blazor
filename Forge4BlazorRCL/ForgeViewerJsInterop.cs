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

        public static async Task<string> LoadDocument(IJSRuntime jsRuntime, string aUrn, string aLocation)
        {
            var aResult = await jsRuntime.InvokeAsync<string>("forgeViewerJsFunctions.loadDocument", new object[] { aUrn, aLocation });
            return aResult;
        }
        public static async Task LoadNode(IJSRuntime jsRuntime, string aViewable, string aLocation)
        {
            await jsRuntime.InvokeAsync<string>("forgeViewerJsFunctions.loadDocumentNode", new object[] { aViewable, aLocation });
        }
        public static async Task LoadFile(IJSRuntime jsRuntime, string aUri, string aLocation)
        {
            await jsRuntime.InvokeAsync<string>("forgeViewerJsFunctions.loadFile", new object[] { aUri, aLocation });
        }

        public static async Task LoadExtension(IJSRuntime jsRuntime, string aExtension, string aLocation)
        {
            await jsRuntime.InvokeAsync<string>("forgeViewerJsFunctions.loadExtension", new object[] { aExtension, aLocation });
        }
        public static async Task RegisterAndActivateTool(IJSRuntime jsRuntime, object aTool, string aLocation)
        {
            await jsRuntime.InvokeAsync<string>("forgeViewerJsFunctions.registerAndActivateTool", new object[] { aTool, aLocation });
        }
        public static async Task DeregisterAndDeactivateTool(IJSRuntime jsRuntime, object aTool, string aLocation)
        {
            await jsRuntime.InvokeAsync<string>("forgeViewerJsFunctions.deregisterAndDeactivateTool", new object[] { aTool, aLocation });
        }

    }
}
