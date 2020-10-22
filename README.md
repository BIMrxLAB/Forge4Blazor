# Forge4Blazor
Making it possible to use Autodesk's Forge Viewer in Blazor.

The app project uses secrets to keep forge client application settings from github.

~~~json
{
  "Forge": {
    "ClientId": "fancy client id",
    "ClientSecret": "fancy client secret",
    "SampleUrn": "example urn to a forge document"
  }
}
~~~
