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
        private ForgeApiService ForgeApiService { get;set;}
        public string Id { get; set; }
        public List<string> Extensions { get; set; }
        public ForgeViewer(IJSRuntime jsRuntime, string id, ForgeApiService aForgeApiService)
        {
            ForgeApiService = aForgeApiService;
            JSRuntime = jsRuntime;
            Id = id;
            Extensions = new List<string>();
        }

        public async Task Start()
        {
            string aToken = ForgeApiService.PublicToken["access_token"].Value<string>();
            await JSRuntime.InvokeAsync<string>("forgeViewerJsFunctions.startViewer", new object[] { aToken, Id });
            OnViewerStarted.Invoke(this, null);
        }

        public EventHandler OnViewerStarted { get; set; }

        public async Task LoadExtensionAsync(string aExtensionName)
        {
            await JSRuntime.InvokeAsync<string>("forgeViewerJsFunctions.loadExtension", aExtensionName, Id);
        }

        public async Task LoadFileAsync(string aUri)
        {
            await JSRuntime.InvokeAsync<string>("forgeViewerJsFunctions.loadFile", aUri, Id);
        }
        public async Task LoadDocumentAsync(string aUrn)
        {
            await JSRuntime.InvokeAsync<string>("forgeViewerJsFunctions.loadDocument", aUrn, Id);
        }

        public async Task LoadNode(string aViewable=null)
        {
            await JSRuntime.InvokeAsync<string>("forgeViewerJsFunctions.loadDocumentNode", aViewable, Id);
        }

        #region Mouse Move
        public async Task AddMouseMoveEvent()
        {
            var dotNetReference = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("forgeViewerJsFunctions.addMouseMoveEvent", new object[] { dotNetReference, Id });
        }
        public EventHandler<ForgeViewerMousePosition> MouseMoved { get; set; }
        //https://blazor-university.com/javascript-interop/calling-dotnet-from-javascript/
        [JSInvokable("PostMouseMoveLocation")]
        public void PostMouseMoveLocation(double cx, double cy, double wx, double wy, double sx, double sy, double sz, string sType)
        {
            MouseMoved.Invoke(this, new ForgeViewerMousePosition(cx, cy, wx, wy, sx, sy, sz, sType));
        }
        #endregion

        #region Mouse Click
        public async Task AddMouseClickEvent()
        {
            var dotNetReference = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("forgeViewerJsFunctions.addMouseClickEvent", new object[] { dotNetReference, Id });
        }
        public EventHandler<ForgeViewerMousePosition> MouseClicked { get; set; }
        [JSInvokable("PostMouseClickLocation")]
        public void PostMouseClickLocation(double cx, double cy, double wx, double wy, double sx, double sy, double sz, string sType)
        {
            MouseClicked.Invoke(this, new ForgeViewerMousePosition(cx, cy, wx, wy, sx, sy, sz, sType));
        }
        #endregion

        #region Autodesk.Extensions.Snapper
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
        #endregion
    }
}
