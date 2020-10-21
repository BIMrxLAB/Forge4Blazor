using Autodesk.Forge;
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

        private static JObject InternalToken { get; set; }
        private static JObject PublicToken { get; set; }

        private static string ClientId { get; set; }
        private static string ClientSecret { get; set; }

        public void SetClientIdAndSecret(string aClientId, string aClientSecret)
        {
            ClientId = aClientId;
            ClientSecret = aClientSecret;
        }
        private static async Task<JObject> Get2LeggedTokenAsync(Scope[] scopes)
        {
            TwoLeggedApi oauth = new TwoLeggedApi();
            string grantType = "client_credentials";
            dynamic bearer = await oauth.AuthenticateAsync(ClientId, ClientSecret, grantType, scopes);
            return JObject.FromObject(bearer);
        }


        /// <summary>
        /// Get access token with public (viewables:read) scope
        /// </summary>
        public static async Task<JObject> GetTokenAsync()
        {
            if (PublicToken == null || PublicToken["ExpiresAt"].Value<DateTime>() < DateTime.UtcNow)
            {
                PublicToken = await Get2LeggedTokenAsync(new Scope[] { Scope.ViewablesRead });
                PublicToken["ExpiresAt"] = DateTime.UtcNow.AddSeconds(PublicToken["expires_in"].Value<double>());
            }
            return JObject.FromObject(PublicToken);
        }

        /// <summary>
        /// Get access token with internal (write) scope
        /// </summary>
        public static async Task<JObject> GetInternalAsync()
        {
            if (InternalToken == null || InternalToken["ExpiresAt"].Value<DateTime>() < DateTime.UtcNow)
            {
                InternalToken = await Get2LeggedTokenAsync(new Scope[] { Scope.BucketCreate, Scope.BucketRead, Scope.DataRead, Scope.DataCreate });
                InternalToken["ExpiresAt"] = DateTime.UtcNow.AddSeconds(InternalToken["expires_in"].Value<double>());
            }

            return JObject.FromObject(InternalToken);
        }

        public async Task<JObject> GetManifestAsync(string urn)
        {
            if (PublicToken == null || PublicToken["ExpiresAt"].Value<DateTime>() > DateTime.UtcNow)
            {

                HttpClient aClient = new HttpClient();
                aClient.BaseAddress = new Uri(@"https://developer.api.autodesk.com/modelderivative/v2/");
                aClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PublicToken["access_token"].Value<string>());
                var aResponse = await aClient.GetAsync($"designdata/{urn}/manifest");
                var aResult = await aResponse.Content.ReadAsStringAsync();
                return JObject.Parse(aResult);
            }

            return null;
        }

    }
}
