using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.ErrorCode;
using Microsoft.Bpst.Shared.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bpst.Actions.AzureActions.Twitter
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

