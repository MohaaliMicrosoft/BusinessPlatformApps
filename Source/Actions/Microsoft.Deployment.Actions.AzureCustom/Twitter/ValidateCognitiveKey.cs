using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.ErrorCode;
using Microsoft.Bpst.Shared.Helpers;
using System.Text;

namespace Microsoft.Bpst.Actions.AzureActions.Twitter
{
    [Export(typeof(IAction))]
    public class ValidateCognitiveKey : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {

            //Request headers
            var subscriptionKey = request.Message["subscriptionKey"].ToString();
            var sentiment = JsonUtility.GetJsonStringFromObject("Sentiment test string");

            HttpClientUtility client = new HttpClientUtility();

            Dictionary<string, string> customHeader = new Dictionary<string, string>();
            customHeader.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            var result = client.ExecuteGenericAsync(HttpMethod.Get, $"https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment", sentiment, "" , customHeader).Result;

            var responseString = result.Content.ReadAsStringAsync().Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var obj = JsonUtility.GetJObjectFromJsonString(responseString);
            }

            //new request


            var sentimentResponse = client.PostAsync(uri, content).Result;


            var sentimentResponseBody = sentimentResponse.Content.ReadAsStringAsync().Result;

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetJObjectFromObject(response));
        }
    }
}
