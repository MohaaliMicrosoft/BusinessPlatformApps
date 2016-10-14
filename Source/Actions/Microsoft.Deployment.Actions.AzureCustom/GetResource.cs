using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.AzureActions
{
    [Export(typeof(IAction))]
    public class GetResource : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            var resource = request.Message["Resource"][0].ToString();
            var resourceType = request.Message["ResourceType"][0].ToString();

            SubscriptionCloudCredentials creds = new TokenCloudCredentials(subscription, token);
            ResourceManagementClient client = new ResourceManagementClient(creds);

            ResourceIdentity identity = new ResourceIdentity(resource, resourceType, "2015-08-01");
            var resourceFound = client.Resources.GetAsync(resourceGroup, identity, new CancellationToken()).Result;
            return new ActionResponse(ActionStatus.Success, JsonUtility.GetJObjectFromObject(resourceFound.Resource));
        }
    }
}
