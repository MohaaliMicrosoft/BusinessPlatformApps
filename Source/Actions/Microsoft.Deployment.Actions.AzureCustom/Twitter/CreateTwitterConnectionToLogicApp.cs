using System.ComponentModel.Composition;
using System.Dynamic;
using System.Net.Http;
using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.Bpst.Shared;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.ErrorCode;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.AzureActions.Twitter
{
    [Export(typeof(IAction))]
    public class CreateTwitterConnectionToLogicApp : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            var location = request.Message["SelectedLocation"][0]["Name"].ToString();


            SubscriptionCloudCredentials creds = new TokenCloudCredentials(subscription, token);
            Microsoft.Azure.Management.Resources.ResourceManagementClient client = new ResourceManagementClient(creds);
            var registeration = client.Providers.RegisterAsync("Microsoft.Web").Result;


            dynamic payload = new ExpandoObject();
            payload.properties = new ExpandoObject();
            payload.properties.displayName = "twitter";
            payload.properties.api = new ExpandoObject();
            payload.properties.api.id = $"subscriptions/{subscription}/providers/Microsoft.Web/locations/{location}/managedApis/twitter";
            payload.location = location;

            HttpResponseMessage connection = new AzureHttpClient(token, subscription, resourceGroup).ExecuteWithSubscriptionAndResourceGroupAsync(HttpMethod.Put,
                "/providers/Microsoft.Web/connections/twitter", "2016-06-01", JsonUtility.GetJsonStringFromObject(payload));

            if (!connection.IsSuccessStatusCode)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromJsonString(connection.Content.ReadAsStringAsync().Result), 
                    null, DefaultErrorCodes.DefaultErrorCode, "Failed to create connection");
            }

            
            // Get Consent links for auth
            payload = new ExpandoObject();
            payload.parameters = new ExpandoObject[1];
            payload.parameters[0] = new ExpandoObject();
            payload.parameters[0].objectId = null;
            payload.parameters[0].tenantId = null;
            payload.parameters[0].parameterName = "token";
            payload.parameters[0].redirectUrl = "https://bpsolutiontemplates.com" + Constants.WebsiteRedirectPath;

            HttpResponseMessage consent = new AzureHttpClient(token, subscription, resourceGroup).ExecuteWithSubscriptionAndResourceGroupAsync(HttpMethod.Post,
                "/providers/Microsoft.Web/connections/twitter/listConsentLinks", "2016-06-01", JsonUtility.GetJsonStringFromObject(payload));

            if (!consent.IsSuccessStatusCode)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromJsonString(connection.Content.ReadAsStringAsync().Result),
                    null, DefaultErrorCodes.DefaultErrorCode, "Failed to get consent");
            }
            var connectiondata = JsonUtility.GetJObjectFromJsonString(connection.Content.ReadAsStringAsync().Result);
            var consentdata = JsonUtility.GetJObjectFromJsonString(consent.Content.ReadAsStringAsync().Result);
            dynamic objectToReturn = new ExpandoObject();
            objectToReturn.Consent = consentdata;
            objectToReturn.Connection = connectiondata;
            objectToReturn.UniqueId = payload.parameters[0].objectId;

            return new ActionResponse(ActionStatus.Success, objectToReturn);
        }
    }
}