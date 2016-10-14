
namespace Microsoft.Deployment.Actions.AzureCustom.Arm
{
    using System.ComponentModel.Composition;
    using System.Threading;
    using Microsoft.Azure;
    using Microsoft.Azure.Management.Resources;
    using Microsoft.Deployment.Common.Actions;

    [Export(typeof(IAction))]
    public class GetArmDeploymentStatus : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();

            var deploymentName = request.Message["DeploymentName"].ToString();

            SubscriptionCloudCredentials creds = new TokenCloudCredentials(subscription, token);
            Microsoft.Azure.Management.Resources.ResourceManagementClient client = new ResourceManagementClient(creds);
            var status = client.Deployments.GetAsync(resourceGroup, deploymentName, new CancellationToken()).Result;

            return new ActionResponse(ActionStatus.Success, status);
        }
    }
}