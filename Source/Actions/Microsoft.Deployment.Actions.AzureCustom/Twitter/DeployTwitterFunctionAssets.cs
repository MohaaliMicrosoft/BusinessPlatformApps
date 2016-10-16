using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.AzureCustom.Twitter
{
    [Export(typeof(IAction))]
    public class DeployTwitterFunctionAssets : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][1]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            var location = request.Message["SelectedLocation"][0]["Name"].ToString();
            var sitename = request.Message["SiteName"][0].ToString();
            var sqlConnectionString = request.Message["SqlConnectionString"][0].ToString();

            AzureHttpClient client = new AzureHttpClient(token, subscription, resourceGroup);
            var jsonBody =
                "{\"files\":{\"run.py\":\"test\"},\"config\":" +
                "{\"" +
                "bindings\":" +
                "[" +
                    "{\"name\":\"req\"," +
                    "\"type\":\"httpTrigger\"," +
                    "\"direction\":\"in\"," +
                    "\"webHookType\":\"genericJson\"," +
                    "\"scriptFile\":\"run.py\"" +
                    "}" +
                 "]," +
                 "\"disabled\":false}}";
            var functionCreated = client.ExecuteWebsiteAsync(HttpMethod.Put, sitename, "/api/functions/TweetProcessingFunction",
                jsonBody);

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
            obj.connectionStrings = new ExpandoObject[1];
            obj.connectionStrings[0] = new ExpandoObject();
            obj.connectionStrings[0].ConnectionString = SqlUtility.GetPythonConnectionString(sqlConnectionString);
            obj.connectionStrings[0].Name = "SQLCONN";
            obj.connectionStrings[0].Type = 2;
            obj.location = location;

            var appSettingCreated = client.ExecuteGenericRequestWithHeaderAsync(HttpMethod.Post, @"https://web1.appsvcux.ext.azure.com/websites/api/Websites/UpdateConfigConnectionStrings",
               JsonUtility.GetJsonStringFromObject(obj)).Result;
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

            var json = JsonUtility.GetJObjectFromJsonString(response);
            json["properties"]["containerSize"] = 1536;

            var postFunction = client.ExecuteWithSubscriptionAndResourceGroupAsync(HttpMethod.Put,
               $"/providers/Microsoft.Web/sites/{sitename}", "2015-08-01", JsonUtility.GetJsonStringFromObject(json));
            response = postFunction.Content.ReadAsStringAsync().Result;

            if (!postFunction.IsSuccessStatusCode)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromJsonString(response), 
                    null, DefaultErrorCodes.DefaultErrorCode, "Error creating appsetting");
            }


            /// Temporary - Remove Tracing from Azure Function

            HttpResponseMessage publishxml = client.ExecuteWithSubscriptionAndResourceGroupAsync(HttpMethod.Post, "/providers/Microsoft.Web/sites/" +
                sitename + "/publishxml", "2015-02-01", string.Empty);
            var publishxmlfile = publishxml.Content.ReadAsStringAsync().Result;

            XDocument doc = XDocument.Parse(publishxmlfile);
            XElement xElement = doc.Element("publishData");

            if (xElement == null)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromStringValue(publishxmlfile), null, DefaultErrorCodes.DefaultErrorCode,
                        "Unable to get publisher profile");
            }

            var publishProfiles = xElement.Elements("publishProfile");
            var profile = publishProfiles.SingleOrDefault(p => p.Attribute("publishMethod").Value == "FTP");

            if (profile == null)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromStringValue(publishxmlfile), null, DefaultErrorCodes.DefaultErrorCode,
                    "Unable to find FTP profile");
            }

            var ftpServer = profile.Attribute("publishUrl").Value;
            var username = profile.Attribute("userName").Value;
            var password = profile.Attribute("userPWD").Value;
            FtpUtility.UploadFile(ftpServer, username, password, request.TemplatePath + "/service/data", "host.json");


            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }
    }
}