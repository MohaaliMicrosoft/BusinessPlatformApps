using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.AzureCustom.Twitter
{
    [Export(typeof(IAction))]
    public class DeployCognitiveServiceText : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            var cognitiveServiceKey = request.DataStore.GetValue("CognitiveServiceKey");

            if (cognitiveServiceKey == "")
            {
                var azureToken = request.DataStore.GetJson("AzureToken")["access_token"].ToString();
                var subscription = request.DataStore.GetJson("SelectedSubscription")["SubscriptionId"].ToString();
                var resourceGroup = request.DataStore.GetValue("SelectedResourceGroup");
                var location = request.DataStore.GetJson("SelectedLocation")["Name"].ToString();

                var deploymentName = request.DataStore.GetValue("DeploymentName");
                var cognitiveServiceName = request.DataStore.GetValue("CognitiveServiceName");
                var skuName = request.DataStore.GetValue("CognitiveSkuName");
               
                var param = new AzureArmParameterGenerator();
                param.AddStringParam("CognitiveServiceName", cognitiveServiceName);
                param.AddStringParam("skuName", skuName);


                SubscriptionCloudCredentials creds = new TokenCloudCredentials(subscription, azureToken);
                Microsoft.Azure.Management.Resources.ResourceManagementClient client = new ResourceManagementClient(creds);
                var registeration = await client.Providers.RegisterAsync("Microsoft.CognitiveServices");

                var armTemplate = JsonUtility.GetJObjectFromJsonString(System.IO.File.ReadAllText(Path.Combine(request.ControllerModel.AppPath, "Service/AzureArm/sentimentCognitiveService.json")));
                var armParamTemplate = JsonUtility.GetJObjectFromObject(param.GetDynamicObject());
                armTemplate.Remove("parameters");
                armTemplate.Add("parameters", armParamTemplate["parameters"]);


                var deployment = new Microsoft.Azure.Management.Resources.Models.Deployment()
                {
                    Properties = new DeploymentPropertiesExtended()
                    {
                        Template = armTemplate.ToString(),
                        Parameters = JsonUtility.GetEmptyJObject().ToString()
                    }
                };

                var validate = await client.Deployments.ValidateAsync(resourceGroup, deploymentName, deployment, new CancellationToken());
                if (!validate.IsValid)
                {
                    return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromObject(validate), null,
                         DefaultErrorCodes.DefaultErrorCode, $"Azure:{validate.Error.Message} Details:{validate.Error.Details}");
                }

                var deploymentItem = await client.Deployments.CreateOrUpdateAsync(resourceGroup, deploymentName, deployment, new CancellationToken());
                return new ActionResponse(ActionStatus.Success, deploymentItem);
            }

            return new ActionResponse(ActionStatus.Success, string.Empty);

        }
    }
}
