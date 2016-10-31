using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.AzureCustom.Twitter
{
    [Export(typeof(IAction))]
    public class ValidateCognitiveKey : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            //Request headers
            var subscriptionKey = request.DataStore.GetValue("subscriptionKey");
            var sentiment = JsonUtility.GetJsonStringFromObject("Sentiment test string");

            HttpClientUtility client = new HttpClientUtility();

            Dictionary<string, string> customHeader = new Dictionary<string, string>();
            customHeader.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            var result = await client.ExecuteGenericAsync(HttpMethod.Get, $"https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment", sentiment, "" , customHeader);

            var responseString = await result.Content.ReadAsStringAsync();

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var obj = JsonUtility.GetJObjectFromJsonString(responseString);
            }

            return new ActionResponse(ActionStatus.Success);
        }
    }
}
