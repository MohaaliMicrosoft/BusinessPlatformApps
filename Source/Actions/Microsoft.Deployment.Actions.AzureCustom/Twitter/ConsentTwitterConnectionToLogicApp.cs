using System.ComponentModel.Composition;
using System.Dynamic;
using System.Net.Http;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.AzureCustom.Twitter
{
    [Export(typeof(IAction))]
    public class ConsentTwitterConnectionToLogicApp : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            var location = request.Message["SelectedLocation"][0]["Name"].ToString();
            var twitterCode = request.Message["TwitterCode"][0].ToString();
            

            dynamic payload = new ExpandoObject();
            
            payload.objectId = null;
            payload.tenantId = null;
            payload.code = twitterCode;

            HttpResponseMessage consent = new AzureHttpClient(token, subscription, resourceGroup).ExecuteWithSubscriptionAndResourceGroupAsync(HttpMethod.Post,
                "/providers/Microsoft.Web/connections/twitter/confirmConsentCode", "2015-08-01-preview", 
                JsonUtility.GetJsonStringFromObject(payload));

            var consentInformation = JsonUtility.GetJObjectFromJsonString(consent.Content.ReadAsStringAsync().Result);
            if (!consent.IsSuccessStatusCode)
            {
                return new ActionResponse(ActionStatus.Failure, consentInformation, null , DefaultErrorCodes.DefaultErrorCode,  "Failed to create connection");
            }


            payload = new ExpandoObject();
            payload.properties = new ExpandoObject();
            payload.properties.displayName = "twitter";
            payload.properties.api = new ExpandoObject();
            payload.properties.api.id = $"subscriptions/{subscription}/providers/Microsoft.Web/locations/{location}/managedApis/twitter";
            payload.location = location;

            HttpResponseMessage connection = new AzureHttpClient(token, subscription, resourceGroup).ExecuteWithSubscriptionAndResourceGroupAsync(HttpMethod.Put,
                "/providers/Microsoft.Web/connections/twitter", "2016-06-01", JsonUtility.GetJsonStringFromObject(payload));

            if (!connection.IsSuccessStatusCode)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromJsonString(connection.Content.ReadAsStringAsync().Result),null, 
                    DefaultErrorCodes.DefaultErrorCode, "Failed to create connection");
            }

            var connectionData = JsonUtility.GetJObjectFromJsonString(connection.Content.ReadAsStringAsync().Result);
            if (connectionData["properties"]["statuses"][0]["status"].ToString() != "Connected")
            {
                return new ActionResponse(ActionStatus.Failure, connectionData);
            }

            return new ActionResponse(ActionStatus.Success, connectionData);
        }
    }
}