using System.ComponentModel.Composition;
using System.Dynamic;
using System.Net.Http;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.AzureCustom
{
    [Export(typeof(IAction))]
    public class CheckAzureWebsiteExists : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var sitename = request.Message["SiteName"][0].ToString();

            dynamic obj = new ExpandoObject();
            obj.hostName = ".azurewebsites.net";
            obj.siteName = sitename;
            obj.subscriptionId = subscription;

            AzureHttpClient client = new AzureHttpClient(token, subscription);
            var statusResponse = client.ExecuteGenericRequestWithHeaderAsync(HttpMethod.Post, @"https://web1.appsvcux.ext.azure.com/websites/api/Websites/ValidateSiteName",
               JsonUtility.GetJsonStringFromObject(obj)).Result;
            var response = statusResponse.Content.ReadAsStringAsync().Result;
            if (!statusResponse.IsSuccessStatusCode)
            {
                return new ActionResponse(ActionStatus.FailureExpected, JsonUtility.GetJsonObjectFromJsonString(response), null,  AzureErrorCodes.AzureWebsiteNameTaken);
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetJObjectFromStringValue(response));
        }
    }
}
