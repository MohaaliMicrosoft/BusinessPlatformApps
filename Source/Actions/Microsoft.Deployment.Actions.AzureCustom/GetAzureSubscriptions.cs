using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Threading;
using Hyak.Common;
using Microsoft.Azure;
using Microsoft.Azure.Subscriptions;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.ErrorCode;
using Microsoft.Bpst.Shared.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bpst.Actions.AzureActions
{
    [Export(typeof(IAction))]
    public class GetAzureSubscriptions : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            if (request.Message == null || request.Message.SelectToken("Token") == null || request.Message["Token"].Type != JTokenType.Array
                || request.Message["Token"][0].SelectToken("access_token") == null)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), null, AzureErrorCodes.AzureLoginFailed);
            }
            var token = request.Message["Token"][0]["access_token"].ToString();

            CloudCredentials creds = new TokenCloudCredentials(token);
            Microsoft.Azure.Subscriptions.SubscriptionClient client = new SubscriptionClient(creds);
            var subscriptionList = client.Subscriptions.ListAsync(new CancellationToken()).Result.Subscriptions.ToList();
            dynamic subscriptionWrapper = new ExpandoObject();
            subscriptionWrapper.value = subscriptionList;
            return new ActionResponse(ActionStatus.Success, subscriptionWrapper);
        }
    }
}