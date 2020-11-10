# Forge4Blazor
Making it possible to use Autodesk's Forge Viewer in Blazor.

## About this Repo

This repo contains 2 projects: 
1) a razor class library (Forge4BlazorRCL) and 
2) a razor web app (Forge4BlazorApp) that references it. 

Razor Class Libraries allow you to "ship" .js, .razor, .cs files to a referencing app 
using 'Add Project Reference' or 'Manage NuGet Packages'.

Native content (e.g. .js files) land in the referencing app's 'www' folder. 
In this case the forgeViewerJsInterop.js file from the RCL will land in the blazor app's static 
'www' directory like so: _content/Forge4BlazorRCL/forgeViewerJsInterop.js

## Reproducing the Blazor App

Here are the few steps to arrive at the Blazor server side app from scratch:

**1) New Blazor Server Side App with project reference to RCL**<br>
Create a new server side blazor app. Add a project reference to the Forge4BlazorRCL project.

**2) Registering the ForgeApiService**<br>
Include the following singleton in the Startup.cs
~~~c#
services.AddSingleton<Forge4BlazorRCL.ForgeApiService>();
~~~

**3) Providing and Setting Forge App Id and Secret**<br>
ClientId and ClientSecret are loaded through the ForgeApiService, for instance by 
appending to the App.razor file:

~~~c#
@inject IConfiguration Configuration
@inject Forge4BlazorRCL.ForgeApiService ForgeApiService
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
Secrets can be accessed by right clicking the blazor app project and selecting 'Manage User Secrets'. 
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

~~~c#
<NavLink class="nav-link" href="@($"viewer/
    {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(aObject.ObjectId))}/
    {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(aObject.Location))}")">
    <span class="oi oi-file" aria-hidden="true"></span> @aObject.ObjectKey
</NavLink>
~~~