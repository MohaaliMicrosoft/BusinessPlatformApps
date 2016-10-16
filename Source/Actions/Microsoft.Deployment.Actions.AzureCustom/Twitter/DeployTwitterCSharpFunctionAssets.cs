using System.ComponentModel.Composition;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Actions.AzureCustom.Twitter
{
    [Export(typeof(IAction))]
    public class DeployTwitterCSharpFunctionAssets : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][1]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            var location = request.Message["SelectedLocation"][0]["Name"].ToString();
            var sitename = request.Message["SiteName"][0].ToString();
            var sqlConnectionString = request.Message["SqlConnectionString"][0].ToString();
            var cognitiveServiceKey = request.Message["CognitiveServiceKey"][0].ToString();

            AzureHttpClient client = new AzureHttpClient(token, subscription, resourceGroup);

            var functionCSharp = System.IO.File.ReadAllText(Path.Combine(request.TemplatePath, "Service/Data/TweetFunctionCSharp.cs"));
            var jsonBody =
                "{\"files\":{\"run.csx\":\"test\"},\"config\":" +
                "{\"" +
                "bindings\":" +
                "[" +
                    "{\"name\":\"req\"," +
                    "\"type\":\"httpTrigger\"," +
                    "\"direction\":\"in\"," +
                    "\"webHookType\":\"genericJson\"," +
                    "\"scriptFile\":\"run.csx\"" +
                    "}" +
                 "]," +
                 "\"disabled\":false}}";

            JObject jsonRequest = JsonUtility.GetJObjectFromJsonString(jsonBody);
            jsonRequest["files"]["run.csx"] = functionCSharp;
            string stringRequest = JsonUtility.GetJsonStringFromObject(jsonRequest); 

            var functionCreated = client.ExecuteWebsiteAsync(HttpMethod.Put, sitename, "/api/functions/TweetProcessingFunction",
            stringRequest);

            string response = functionCreated.Content.ReadAsStringAsync().Result;
            if (!functionCreated.IsSuccessStatusCode)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromJsonString(response),
                    null, DefaultErrorCodes.DefaultErrorCode, "Error creating function");
            }
            
            dynamic obj = new ExpandoObject();
            obj.subscriptionId = subscription;
            obj.siteId = new ExpandoObject();
            obj.siteId.Name = sitename;
            obj.siteId.ResourceGroup = resourceGroup;
            obj.connectionStrings = new ExpandoObject[2];
            obj.connectionStrings[0] = new ExpandoObject();
            obj.connectionStrings[0].ConnectionString = sqlConnectionString;
            obj.connectionStrings[0].Name = "connectionString";
            obj.connectionStrings[0].Type = 2;
            obj.connectionStrings[1] = new ExpandoObject();
            obj.connectionStrings[1].ConnectionString = cognitiveServiceKey;
            obj.connectionStrings[1].Name = "subscriptionKey";
            obj.connectionStrings[1].Type = 2;
            obj.location = location;

            var appSettingCreated = client.ExecuteGenericRequestWithHeaderAsync(HttpMethod.Post, @"https://web1.appsvcux.ext.azure.com/websites/api/Websites/UpdateConfigConnectionStrings",
            JsonUtility.GetJsonStringFromObject(obj));
            response = appSettingCreated.Content.ReadAsStringAsync().Result;
            if (!appSettingCreated.IsSuccessStatusCode)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromJsonString(response),
                    null, DefaultErrorCodes.DefaultErrorCode, "Error creating appsetting");
            }

            var getFunction = client.ExecuteWithSubscriptionAndResourceGroupAsync(HttpMethod.Get,
            $"/providers/Microsoft.Web/sites/{sitename}", "2015-08-01", string.Empty);
            response = getFunction.Content.ReadAsStringAsync().Result;

            if (!getFunction.IsSuccessStatusCode)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromJsonString(response),
                    null, DefaultErrorCodes.DefaultErrorCode, "Error creating appsetting");
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }
    }
}
