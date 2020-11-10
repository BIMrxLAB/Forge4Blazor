# Forge4Blazor
Making it possible to hack Autodesk's Forge Viewer in .NET Core using Blazor and C#. The component supports bindings for a number of use-cases and is used like shown below:

~~~c#
<ForgeViewerComponent id="id1" 
                        style="position: relative; height: calc(100% - 100px); width: calc(100% - 0px);"
                        OnViewerInitializedCallback="GetViewer" 
                        UseSnapper="snapIsOn" OnMouseMove="OnMouseMove" OnMouseClick="OnMouseClick"
                        >
</ForgeViewerComponent>
~~~

## About this Repo

This reference code base contains 2 projects: 
1) a razor class library (Forge4BlazorRCL) and 
2) a server side blazor app (Forge4BlazorApp) that references it. 

Razor Class Libraries allow you to "ship" .js, .razor, .cs files to a referencing app 
using 'Add Project Reference' or 'Manage NuGet Packages'.

Native content (e.g. .js files) land in the referencing app's 'www' folder. 
In this case the forgeViewerJsInterop.js file from the RCL will land in the blazor app's static 
'www' directory like so: _content/Forge4BlazorRCL/forgeViewerJsInterop.js

## What You Need - byoOSS

This project uses a registered Forge app and OSS buckets - this way we can skip sign-on, use a 2-legged token 
and keep things simple. You can use the razor component for BIM360 content, but you'll have to 
provide / share the means to get the token to start your viewer. Yay, 3-legged token and Auth.

## Reproducing the Blazor App

Here are the few steps to arrive at the Blazor server side app from scratch:

**1) New Blazor Server Side App with project reference to RCL**<br>
Create a new server side blazor app. Add a project reference to the Forge4BlazorRCL project.

**2) A little _Imports action and registering the ForgeApiService**<br>
Add a using statement in the _Imports.razor file - you'll use this a lot:

~~~c#
@using Forge4BlazorRCL;
~~~

Include the following singleton in the Startup.cs:

~~~c#
services.AddSingleton<ForgeApiService>();
~~~

**3) Providing and Setting Forge App Id and Secret**<br>
ClientId and ClientSecret are loaded through the ForgeApiService, for instance by 
appending to the App.razor file:

~~~c#
@inject IConfiguration Configuration
@inject ForgeApiService ForgeApiService
@code{
    protected override async Task OnInitializedAsync()
    {
        ForgeApiService.SetClientIdAndSecret(
            Configuration.GetValue<string>("Forge:ClientId"),
            Configuration.GetValue<string>("Forge:ClientSecret")
        );
    }
}
~~~

The app project uses secrets to keep forge client application settings seperated from github. 
Secrets can be accessed by right clicking the Forge4BlazorApp project and selecting 'Manage User Secrets'. 
The _C:\ Users\ user_name\ AppData\ Roaming\ Microsoft\ UserSecrets\ some_guid\ secrets.json_  should look like this:

~~~json
{
  "Forge": {
    "ClientId": "fancy client id",
    "ClientSecret": "fancy client secret"
  }
}
~~~

**4) Loading Buckets and BucketObjects**

We load the buckets and their content from the registered forge app directly in the NavMenu.razor file using the ForgeApiService:

~~~c#
@using Autodesk.Forge.Model;
@using Forge4BlazorRCL;
@inject ForgeApiService ForgeApiService;

Buckets Buckets { get; set; }
Dictionary<string, BucketObjects> BucketObjects { get; set; }
Dictionary<string, bool> BucketIsCollapsed { get; set; }
protected override async Task OnInitializedAsync()
{
    Buckets = await ForgeApiService.GetBucketsAsync();
    BucketObjects = new Dictionary<string, BucketObjects>();
    BucketIsCollapsed = new Dictionary<string, bool>();
    foreach (var aBucket in Buckets.Items)
    {
        BucketIsCollapsed.Add(aBucket.BucketKey, true);
        BucketObjects.Add(aBucket.BucketKey, await ForgeApiService.GetBucketObjectsAsync(aBucket));
    }
}
~~~

**5) Viewer.razor page using ForgeViewerComponent**

We provide navigation to a page from the NavMenu.razor, where we encode the resource's ObjectId (the urn) and the Location (the uri) as Base64String.
When translated models are used, we use the urn. The Autodesk.PDF extension conviniently loads un-translated pdf files directly from their location uri.

In the Viewer.razor:
~~~c#
@page "/viewer/{urn64}/{uri64}"
~~~

The link to it from the NavMenu.razor page:
~~~c#
<NavLink class="nav-link" href="@($"viewer/
    {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(aObject.ObjectId))}/
    {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(aObject.Location))}")">
    <span class="oi oi-file" aria-hidden="true"></span> @aObject.ObjectKey
</NavLink>
~~~

Here's how the ForeViewer razor component is used in the html markup:
~~~c#
<ForgeViewerComponent id="id1" 
                        style="position: relative; height: calc(100% - 100px); width: calc(100% - 0px);"
                        OnViewerInitializedCallback="GetViewer" 
                        UseSnapper="snapIsOn" OnMouseMove="OnMouseMove" OnMouseClick="OnMouseClick"
                        >
</ForgeViewerComponent>
~~~
We use the id of the component to identify the \<div> that hosts the forge viewer. This also allows you to have multiple viewers on a single page. We 
provide optional styling to control spacing of the viewer. 
At the moment we provide a callback to get a ForgeViewer object to more directly interact with the viewer. 
Also, note how we use bindings to toggle snapper on/off, and get our hands on mouse move and click callbacks. 

Here's a sample of how we get the viewer started:

~~~c#
// getting the viewer object after initialization of the component
ForgeViewer myViewer { get; set; }
private void GetViewer(ForgeViewer aViewer)
{
    myViewer = aViewer;
}

// starting the viewer and loading either urn or pdf content
bool snapIsOn { get; set; }
protected override async Task OnParametersSetAsync()
{
    // we use the manifest to check if the resource was translated
    JObject manifestJObject = await ForgeApiService.GetManifestAsync(urn64);
    var manifestTxt = Newtonsoft.Json.JsonConvert.SerializeObject(manifestJObject, Newtonsoft.Json.Formatting.Indented);

    snapIsOn = false;
    await myViewer.Start();

    // un-translated pdf's don't have a manifest
    if (manifestJObject == null && uri.ToLower().EndsWith("pdf"))
    {
        await myViewer.LoadExtensionAsync("Autodesk.PDF");
        await myViewer.LoadFileAsync(uri);
    }
    else
    {
        await myViewer.LoadDocumentAsync($"urn:{urn64}");
        await myViewer.LoadNode();
    }
}

// example for the mouse move callback
ForgeViewerMousePosition ForgeViewerMouseMovePosition = new ForgeViewerMousePosition();
private void OnMouseMove(ForgeViewerMousePosition aFVMousePosition)
{
    ForgeViewerMouseMovePosition = aFVMousePosition;
}
~~~

There is a small class for MousePosition callbacks, ForgeViewerMousePosition.cs, with just the following properties:

~~~c#
public ForgeViewerMousePosition(double cx, double cy, double wx, double wy, double sx, double sy, double sz, string sType)
{
    CanvasX = cx;
    CanvasY = cy;
    WorldX = wx;
    WorldY = wy;
    SnapX = sx;
    SnapY = sy;
    SnapZ = sz;
    SnapType = sType;
}
~~~

Note: a small adjustment in the site.css to make room for the viewer. This works well 
in standard blazor projects - otherwise you end up with a perfectly fine zero size viewer 
you can't see...
~~~css
.content {
    padding-top: 1.1rem;
    height: calc(100% - 56px);
}
~~~