using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Forge4BlazorRCL
{
    public class ForgeViewer
    {
        private readonly IJSRuntime JSRuntime;

        public string Id { get; set; }
        public List<string> Extensions { get; set; }
        public ForgeViewer(IJSRuntime jsRuntime, string id)
        {
            JSRuntime = jsRuntime;
            Id = id;
            Extensions = new List<string>();
        }

        public async Task Start()
        {
            JObject aToken = await ForgeApiService.GetTokenAsync();
            await ForgeViewerJsInterop.StartViewer(JSRuntime, aToken["access_token"].Value<string>(), Id);
        }

        public async Task LoadExtensionAsync(string aExtensionName)
        {
            await JSRuntime.InvokeAsync<string>("forgeViewerJsFunctions.loadExtension", aExtensionName, Id);
        }

        public async Task LoadModelAsync(string aUri)
        {
            await JSRuntime.InvokeAsync<string>("forgeViewerJsFunctions.loadFile", aUri, Id);
        }
    }
}
