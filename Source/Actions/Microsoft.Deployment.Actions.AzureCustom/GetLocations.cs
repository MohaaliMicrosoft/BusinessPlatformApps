using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Threading;
using Hyak.Common;
using Microsoft.Azure;
using Microsoft.Azure.Subscriptions;
using Microsoft.Bpst.Shared.Actions;

namespace Microsoft.Bpst.Actions.AzureActions
{
    [Export(typeof(IAction))]
    public class GetLocations : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            CloudCredentials creds = new TokenCloudCredentials(token);
            Microsoft.Azure.Subscriptions.SubscriptionClient client = new SubscriptionClient(creds);
            var locationsList = client.Subscriptions.ListLocationsAsync(subscription, new CancellationToken()).Result.Locations.ToList();
            dynamic wrapper = new ExpandoObject();
            wrapper.value = locationsList;
            return new ActionResponse(ActionStatus.Success, wrapper);
        }
    }
}