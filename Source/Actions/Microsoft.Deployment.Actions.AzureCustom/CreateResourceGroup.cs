using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.Deployment.Common.Actions;

namespace Microsoft.Deployment.Actions.AzureCustom
{
    [Export(typeof(IAction))]
    public class CreateResourceGroup: BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            var location = request.Message["SelectedLocation"][0]["Name"].ToString();

            SubscriptionCloudCredentials creds = new TokenCloudCredentials(subscription, token);
            Microsoft.Azure.Management.Resources.ResourceManagementClient client = new ResourceManagementClient(creds);
            var param = new ResourceGroup {Location = location};
            var createdResourceGroup = client.ResourceGroups.CreateOrUpdateAsync(resourceGroup, param, new CancellationToken()).Result;
            
            return new ActionResponse(ActionStatus.Success, createdResourceGroup);
        }
    }
}