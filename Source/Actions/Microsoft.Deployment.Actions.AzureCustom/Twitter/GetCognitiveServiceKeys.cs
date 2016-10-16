using System.ComponentModel.Composition;
using System.Net.Http;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Actions.AzureCustom.Twitter
{
    [Export(typeof(IAction))]
    public class GetCognitiveServiceKeys : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][1]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            var cognitiveServiceName = request.Message["CognitiveServiceName"][0].ToString();

            AzureHttpClient client = new AzureHttpClient(token, subscription, resourceGroup);        

            var response = client.ExecuteWithSubscriptionAndResourceGroupAsync(HttpMethod.Post, $"providers/Microsoft.CognitiveServices/accounts/{cognitiveServiceName}/listKeys", "2016-02-01-preview", string.Empty);
            if (response.IsSuccessStatusCode)
            {
                var subscriptionKeys = JsonUtility.GetJObjectFromJsonString(response.Content.ReadAsStringAsync().Result);

                JObject cognitiveServiceKey = new JObject();
                cognitiveServiceKey.Add("CognitiveServiceKey", subscriptionKeys["key1"].ToString());  
                return new ActionResponse(ActionStatus.Success, cognitiveServiceKey);
            }

            return new ActionResponse(ActionStatus.Failure);
        }
    }
}

