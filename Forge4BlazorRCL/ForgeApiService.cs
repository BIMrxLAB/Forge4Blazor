using Autodesk.Forge;
using Autodesk.Forge.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Forge4BlazorRCL
{
    public class ForgeApiService
    {

        public JObject InternalToken { get; set; }

        public JObject PublicToken { get; set; }

        private static string ClientId { get; set; }
        private static string ClientSecret { get; set; }

        public static BucketsApi bucketApi = new BucketsApi();
        public static ObjectsApi objectsApi = new ObjectsApi();

        public ForgeApiService()
        {

        }
        public void SetClientIdAndSecret(string aClientId, string aClientSecret)
        {
            ClientId = aClientId;
            ClientSecret = aClientSecret;
        }
        private async Task<JObject> Get2LeggedTokenAsync(Scope[] scopes)
        {
            TwoLeggedApi oauth = new TwoLeggedApi();
            string grantType = "client_credentials";
            dynamic bearer = await oauth.AuthenticateAsync(ClientId, ClientSecret, grantType, scopes);
            return JObject.FromObject(bearer);
        }


        /// <summary>
        /// Get access token with public (viewables:read) scope
        /// </summary>
        public async Task SetPublicTokenAsync()
        {
            if (PublicToken == null || PublicToken["ExpiresAt"].Value<DateTime>() < DateTime.UtcNow)
            {
                PublicToken = await Get2LeggedTokenAsync(new Scope[] { Scope.BucketRead, Scope.DataRead, Scope.ViewablesRead });
                PublicToken["ExpiresAt"] = DateTime.UtcNow.AddSeconds(PublicToken["expires_in"].Value<double>());
            }
        }

        /// <summary>
        /// Get access token with internal (write) scope
        /// </summary>
        public async Task SetInternalTokenAsync()
        {
            if (InternalToken == null || InternalToken["ExpiresAt"].Value<DateTime>() < DateTime.UtcNow)
            {
                InternalToken = await Get2LeggedTokenAsync(new Scope[] { Scope.BucketCreate, Scope.BucketRead, Scope.DataRead, Scope.DataCreate });
                InternalToken["ExpiresAt"] = DateTime.UtcNow.AddSeconds(InternalToken["expires_in"].Value<double>());
            }
        }

        public async Task<JObject> GetManifestAsync(string urn)
        {
            await SetPublicTokenAsync();
            HttpClient aClient = new HttpClient
            {
                BaseAddress = new Uri(@"https://developer.api.autodesk.com/modelderivative/v2/")
            };
            aClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PublicToken["access_token"].Value<string>());
            var aResponse = await aClient.GetAsync($"designdata/{urn}/manifest");
            var aResult = await aResponse.Content.ReadAsStringAsync();
            return aResult=="" ? null : JObject.Parse(aResult);
        }

        public async Task<Buckets> GetBucketsAsync()
        {
            await SetPublicTokenAsync();
            bucketApi.Configuration.AccessToken = PublicToken["access_token"].Value<string>();
            var bucketsDynamic = await bucketApi.GetBucketsAsync();
            var buckets = bucketsDynamic.ToObject<Buckets>();
            return buckets;
        }
        public async Task<BucketObjects> GetBucketObjectsAsync(BucketsItems aBucket)
        {
            await SetPublicTokenAsync();
            objectsApi.Configuration.AccessToken = PublicToken["access_token"].Value<string>();
            var bucketObjectsDynamic = await objectsApi.GetObjectsAsync(aBucket.BucketKey);
            var bucketObjects = bucketObjectsDynamic.ToObject<BucketObjects>();
            return bucketObjects;
        }

    }
}
