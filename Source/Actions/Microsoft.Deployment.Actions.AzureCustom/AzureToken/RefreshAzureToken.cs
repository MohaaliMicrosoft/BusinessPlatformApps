using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Deployment.Common;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Actions.AzureCustom.AzureToken
{
    [Export(typeof(IAction))]
    [Export(typeof(IActionRequestInterceptor))]
    public class RefreshAzureToken : BaseAction, IActionRequestInterceptor
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string refresh_token = request.Message["Token"][0]["refresh_token"].ToString();
            string aadTenant = request.Message["AADTenant"][0].ToString();

            string tokenUrl = string.Format(Constants.AzureTokenUri, aadTenant);
            HttpClient client = new HttpClient();

            var builder = GetTokenUri(refresh_token, Constants.AzureManagementCoreApi, request.WebsiteRootUrl);
            var content = new StringContent(builder.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var response = client.PostAsync(new Uri(tokenUrl), content).Result.Content.ReadAsStringAsync().Result;

            builder = GetTokenUri(refresh_token, Constants.AzureManagementCoreApi, request.WebsiteRootUrl);
            content = new StringContent(builder.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var response2 = client.PostAsync(new Uri(tokenUrl), content).Result.Content.ReadAsStringAsync().Result;

            var primaryResponse = JsonUtility.GetJsonObjectFromJsonString(response);
            var secondaryResponse = JsonUtility.GetJsonObjectFromJsonString(response2);
            JArray array = new JArray() { primaryResponse, secondaryResponse };

            var obj = new JObject(new JProperty("Token", array));
            return new ActionResponse(ActionStatus.Success, obj);
        }

        private static StringBuilder GetTokenUri(string refresh_token, string uri, string rootUrl)
        {
            Dictionary<string, string> message = new Dictionary<string, string>
            {
                {"refresh_token", refresh_token},
                {"client_id", Constants.MicrosoftClientId},
                {"client_secret", Uri.EscapeDataString(Constants.MicrosoftClientSecret)},
                {"resource", Uri.EscapeDataString(uri)},
                {"grant_type", "refresh_token"}
            };

            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, string> keyValuePair in message)
            {
                builder.Append(keyValuePair.Key + "=" + keyValuePair.Value);
                builder.Append("&");
            }
            return builder;
        }

        public InterceptorStatus CanIntercept(IAction actionToExecute, ActionRequest request)
        {
            if (request.Message.SelectToken("Token") != null)
            {
                return InterceptorStatus.Intercept;
            }
            return InterceptorStatus.Skipped;
        }

        public ActionResponse Intercept(IAction actionToExecute, ActionRequest request)
        {
            var tokenRefreshResponse = ExecuteAction(request);
            if (tokenRefreshResponse.Status == ActionStatus.Success)
            {
                request.Message["Token"] = tokenRefreshResponse.Response["Token"];
            }

            return tokenRefreshResponse;
        }
    }
}
