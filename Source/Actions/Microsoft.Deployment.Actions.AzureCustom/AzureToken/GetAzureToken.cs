using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Deployment.Common;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Actions.AzureCustom.AzureToken
{
    [Export(typeof(IAction))]
    public class GetAzureToken : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string code = request.Message["code"].ToString();
            var aadTenant = request.Message["AADTenant"][0].ToString();

            string tokenUrl = string.Format(Constants.AzureTokenUri, aadTenant);
            HttpClient client = new HttpClient();

            var builder = GetTokenUri(code, Constants.AzureManagementCoreApi, request.WebsiteRootUrl);
            var content = new StringContent(builder.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var response = client.PostAsync(new Uri(tokenUrl), content).Result.Content.ReadAsStringAsync().Result;

            builder = GetTokenUri(code, Constants.AzureManagementCoreApi, request.WebsiteRootUrl);
            content = new StringContent(builder.ToString());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var response2 = client.PostAsync(new Uri(tokenUrl), content).Result.Content.ReadAsStringAsync().Result;

            var primaryResponse = JsonUtility.GetJsonObjectFromJsonString(response);
            var secondaryResponse = JsonUtility.GetJsonObjectFromJsonString(response2);
            JArray array = new JArray() { primaryResponse, secondaryResponse };

            var obj = new JObject(new JProperty("Token", array));

            if (primaryResponse.SelectToken("error") != null)
            {
                return new ActionResponse(ActionStatus.Failure, obj, null, 
                    DefaultErrorCodes.DefaultLoginFailed, 
                    primaryResponse.SelectToken("error_description")?.ToString());
            }

            return new ActionResponse(ActionStatus.Success, obj);
        }

        private static StringBuilder GetTokenUri(string code, string uri, string rootUrl)
        {
            Dictionary<string, string> message = new Dictionary<string, string>
            {
                {"code", code},
                {"client_id", Constants.MicrosoftClientId},
                {"client_secret", Uri.EscapeDataString(Constants.MicrosoftClientSecret)},
                {"resource", Uri.EscapeDataString(uri)},
                {"redirect_uri", Uri.EscapeDataString(rootUrl + Constants.WebsiteRedirectPath)},
                {"grant_type", "authorization_code"}
            };

            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, string> keyValuePair in message)
            {
                builder.Append(keyValuePair.Key + "=" + keyValuePair.Value);
                builder.Append("&");
            }
            return builder;
        }
    }
}