using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.Actions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.TestHarness.Helpers
{
    public class TokenUtillity
    {
        private static JObject token = null;

        public static JObject GetAzureToken()
        {
            if (token != null)
            {
                return token;
            }
            return TokenUtillity.GetAzureToken("common");
        }

        public static JObject GetAzureToken(string tenant)
        {
            if (token != null)
            {
                return token;
            }

            DataStoreMock dataStore = new DataStoreMock();
            dataStore.AddItemToDataStore("Azure", "AADTenant", tenant);
            var payload = dataStore.GetDataStore();
            var response = TestService.Instance.ExecuteAction("Microsoft-GetAzureAuthUri", payload, null,true);

            string code = OAuthUtility.OpenBrowserAndGetCode(response.Response["value"].ToString(), TestService.TestRedirectUrl,
               "code", true);
            payload.Add("code", code);


            response = TestService.Instance.ExecuteAction("Microsoft-GetAzureToken", payload , null, true); ;

            TokenUtillity.token = response.Response;
            return response.Response;
        }

        public static DataStoreMock GetDataStoreWithTokenSubscriptionLocation()
        {
            var token = TokenUtillity.GetAzureToken("common");
            DataStoreMock dataStore = new DataStoreMock();
            dataStore.AddObjectToDataStore("Azure", token);
            dataStore.AddItemToDataStore("Azure", "AADTenant", "common");
            var payload = dataStore.GetDataStore();

            //Get Subscription
            var response = TestService.Instance.ExecuteAction("Microsoft-GetAzureSubscriptions", payload);
            // Get location
            dataStore.AddItemToDataStore("Azure", "SelectedSubscription", response.Response.SelectToken("value")[5]);
            payload = dataStore.GetDataStore();
            response = TestService.Instance.ExecuteAction("Microsoft-GetLocations", payload);

            dataStore.AddItemToDataStore("Azure", "SelectedLocation", response.Response.SelectToken("value")[5]);
            return dataStore;
        }
    }
}
