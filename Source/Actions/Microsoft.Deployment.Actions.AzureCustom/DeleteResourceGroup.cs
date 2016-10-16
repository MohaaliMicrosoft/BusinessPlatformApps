using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.AzureCustom
{
    [Export(typeof(IAction))]
    public class DeleteResourceGroup: BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();

            SubscriptionCloudCredentials creds = new TokenCloudCredentials(subscription, token);
            Microsoft.Azure.Management.Resources.ResourceManagementClient client = new ResourceManagementClient(creds);

            try
            {
                var delete = client.ResourceGroups.DeleteAsync(resourceGroup, new CancellationToken()).Result;
            }
            catch
            {
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());

        }
    }
}