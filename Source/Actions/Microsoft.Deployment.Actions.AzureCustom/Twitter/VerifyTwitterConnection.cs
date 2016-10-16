using System.ComponentModel.Composition;
using System.Net.Http;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.AzureCustom.Twitter
{
    [Export(typeof(IAction))]
    public class VerifyTwitterConnection : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {

            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            var location = request.Message["SelectedLocation"][0]["Name"].ToString();

            HttpResponseMessage connection =
                new AzureHttpClient(token, subscription, resourceGroup).ExecuteWithSubscriptionAndResourceGroupAsync(HttpMethod.Get,
                    "/providers/Microsoft.Web/connections/twitter", "2015-08-01-preview", string.Empty);

            if (!connection.IsSuccessStatusCode)
            {
                return new ActionResponse(ActionStatus.FailureExpected,
                    JsonUtility.GetJObjectFromJsonString(connection.Content.ReadAsStringAsync().Result),null,DefaultErrorCodes.DefaultErrorCode,
                    "Failed to get consent");
            }

            var connectionData = JsonUtility.GetJObjectFromJsonString(connection.Content.ReadAsStringAsync().Result);
            if (connectionData["properties"]["statuses"][0]["status"].ToString() != "Connected")
            {
                return new ActionResponse(ActionStatus.FailureExpected, connectionData);
            }

            return new ActionResponse(ActionStatus.Success, connectionData);
        }
    }
}

