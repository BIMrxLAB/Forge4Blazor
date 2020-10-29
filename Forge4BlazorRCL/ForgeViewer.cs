using Microsoft.AspNetCore.Components;
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

        public async Task AddMouseMoveEvent()
        {
            var dotNetReference = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("forgeViewerJsFunctions.addMouseMoveEvent", new object[] { dotNetReference, Id });
        }
        public EventHandler<Tuple<double, double>> XYChanged { get; set; }
        //https://blazor-university.com/javascript-interop/calling-dotnet-from-javascript/
        [JSInvokable("PostMouseMoveLocation")]
        public void PostMouseMoveLocation(double x, double y)
        {
            XYChanged.Invoke(this, new Tuple<double, double>(x, y));
        }

        public async Task AddMouseClickEvent()
        {
            var dotNetReference = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("forgeViewerJsFunctions.addMouseClickEvent", new object[] { dotNetReference, Id });
        }
        public EventHandler<Tuple<double, double>> XYClicked { get; set; }
        [JSInvokable("PostMouseClickLocation")]
        public void PostMouseClickLocation(double x, double y)
        {
            XYClicked.Invoke(this, new Tuple<double, double>(x, y));
        }
        public async Task AddMouseSnapperClickEvent()
        {
            var dotNetReference = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("forgeViewerJsFunctions.addMouseSnapperClickEvent", new object[] { dotNetReference, Id });
        }
        public EventHandler<Tuple<double, double>> XYSnapperClicked { get; set; }
        [JSInvokable("PostMouseSnapperClickLocation")]
        public void PostMouseSnapperClickLocation(double x, double y)
        {
            XYSnapperClicked.Invoke(this, new Tuple<double, double>(x, y));
        }

        public async Task MakeSnapper()
        {
            await JSRuntime.InvokeAsync<string>("forgeViewerJsFunctions.makeSnapper", new object[] { Id });
        }
        public async Task DestroySnapper()
        {
            await JSRuntime.InvokeAsync<string>("forgeViewerJsFunctions.destroySnapper", new object[] { Id });
        }
        public async Task RegisterAndActivateSnapper()
        {
            await JSRuntime.InvokeAsync<string>("forgeViewerJsFunctions.registerAndActivateSnapper", new object[] { Id });
        }
        public async Task DeregisterAndDeactivateSnapper()
        {
            await JSRuntime.InvokeAsync<string>("forgeViewerJsFunctions.deregisterAndDeactivateSnapper", new object[] { Id });
        }

    }
}
