namespace Microsoft.Bpst.Actions.AzureActions.Arm
{
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Threading;
    using Microsoft.Azure;
    using Microsoft.Azure.Management.Resources.Models;
    using Microsoft.Bpst.Shared.Actions;
    using ResourceManagementClient = Microsoft.Azure.Management.Resources.ResourceManagementClient;

    [Export(typeof(IAction))]
    public class WaitForArmDeploymentStatus : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            var deploymentName = request.Message["DeploymentName"].ToString();

            SubscriptionCloudCredentials creds = new TokenCloudCredentials(subscription, token);
            Microsoft.Azure.Management.Resources.ResourceManagementClient client = new ResourceManagementClient(creds);
            
            while (true)
            {
                Thread.Sleep(5000);
                var status = client.Deployments.GetAsync(resourceGroup, deploymentName, new CancellationToken()).Result;
                var operations = client.DeploymentOperations.ListAsync(resourceGroup, deploymentName, new DeploymentOperationsListParameters(),new CancellationToken()).Result;
                var provisioningState = status.Deployment.Properties.ProvisioningState;

                if (provisioningState == "Accepted" || provisioningState == "Running")
                    continue;

                if (provisioningState == "Succeeded")
                    return new ActionResponse(ActionStatus.Success, operations);

                var operation = operations.Operations.First(p => p.Properties.ProvisioningState == ProvisioningState.Failed);
                var operationFailed = client.DeploymentOperations.GetAsync(resourceGroup, deploymentName, operation.OperationId, new CancellationToken()).Result;

                return new ActionResponse(ActionStatus.Failure, operationFailed);
            }
        }
    }
}
