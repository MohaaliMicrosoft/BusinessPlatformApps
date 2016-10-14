using System.ComponentModel.Composition;
using System.Net.Http;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.AzureCustom.Twitter
{
    [Export(typeof(IAction))]
    public class RunLogicAppHistorical : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][1]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            var LogicAppName = request.Message["LogicAppNameHistorical"][0].ToString();

            AzureHttpClient client = new AzureHttpClient(token, subscription, resourceGroup);

            var response = client.ExecuteWithSubscriptionAndResourceGroupAsync(HttpMethod.Post, $"providers/Microsoft.Logic/workflows/{LogicAppName}/triggers/manual/listCallbackUrl", "2016-06-01", string.Empty);
            if (!response.IsSuccessStatusCode)
            {
                return new ActionResponse(ActionStatus.Failure);
            }

            var postUrl = JsonUtility.GetJObjectFromJsonString(response.Content.ReadAsStringAsync().Result);
            
            response = client.ExecuteGenericRequestNoHeaderAsync(HttpMethod.Post, postUrl["value"].ToString(), string.Empty);

            return new ActionResponse(ActionStatus.Success);

        }
    }
}

