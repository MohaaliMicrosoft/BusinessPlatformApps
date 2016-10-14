//-----------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.Bpst.Actions.AzureActions.Arm
{
    using System.ComponentModel.Composition;
    using System.Threading;
    using Microsoft.Azure;
    using Microsoft.Azure.Management.Resources;
    using Microsoft.Azure.Management.Resources.Models;
    using Microsoft.Bpst.Shared.Actions;
    using Microsoft.Bpst.Shared.ErrorCode;
    using Microsoft.Bpst.Shared.Helpers;

    [Export(typeof(IAction))]
    public class DeployArmTemplate : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            var deploymentName = request.Message["DeploymentName"][0].ToString();

            // Read from file
            var armTemplate = request.Message["ArmTemplate"][0].ToString();
            var armParamTemplate = request.Message["ArmParamTemplate"][0].ToString();

            SubscriptionCloudCredentials creds = new TokenCloudCredentials(subscription, token);
            ResourceManagementClient client = new ResourceManagementClient(creds);

            var deployment = new Microsoft.Azure.Management.Resources.Models.Deployment()
            {
                Properties = new DeploymentPropertiesExtended()
                {
                    Template = armTemplate,
                    Parameters = armParamTemplate
                }
            };

            var validate = client.Deployments.ValidateAsync(resourceGroup, deploymentName, deployment, new CancellationToken()).Result;
            if (!validate.IsValid)
                return new ActionResponse(
                    ActionStatus.Failure,
                    JsonUtility.GetJObjectFromObject(validate),
                    null,
                    DefaultErrorCodes.DefaultErrorCode,
                    $"Azure:{validate.Error.Message} Details:{validate.Error.Details}");

            var deploymentItem = client.Deployments.CreateOrUpdateAsync(resourceGroup, deploymentName, deployment, new CancellationToken()).Result;
            return new ActionResponse(ActionStatus.Success, deploymentItem);
        }
    }
}