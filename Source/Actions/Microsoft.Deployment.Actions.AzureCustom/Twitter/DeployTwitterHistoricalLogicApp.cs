﻿using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.ErrorCode;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.AzureActions.Twitter
{
    [Export(typeof(IAction))]
    public class DeployTwitterHistoricalLogicApp : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            var deploymentName = request.Message["DeploymentName"].ToString();
            var sitename = request.Message["SiteName"][0].ToString();
            var location = request.Message["SelectedLocation"][0]["Name"].ToString();
            var search = request.Message["SearchQuery"][0].ToString();
            var logicAppNameHistorical = request.Message["LogicAppNameHistorical"][0].ToString();

            search = search.StartsWith("@") ? "@" + search : search;

            var param = new AzureArmParameterGenerator();
            param.AddStringParam("sitename", sitename);
            param.AddStringParam("resourcegroup", resourceGroup);
            param.AddStringParam("subscription", subscription);
            param.AddStringParam("search", search);
            param.AddStringParam("LogicAppName", logicAppNameHistorical);

            var armTemplate = JsonUtility.GetJObjectFromJsonString(System.IO.File.ReadAllText(Path.Combine(request.TemplatePath, "Service/AzureArm/logicappHistorical.json")));
            var armParamTemplate = JsonUtility.GetJObjectFromObject(param.GetDynamicObject());
            armTemplate.Remove("parameters");
            armTemplate.Add("parameters", armParamTemplate["parameters"]);

            SubscriptionCloudCredentials creds = new TokenCloudCredentials(subscription, token);
            Microsoft.Azure.Management.Resources.ResourceManagementClient client = new ResourceManagementClient(creds);


            var deployment = new Microsoft.Azure.Management.Resources.Models.Deployment()
            {
                Properties = new DeploymentPropertiesExtended()
                {
                    Template = armTemplate.ToString(),
                    Parameters = JsonUtility.GetEmptyJObject().ToString()
                }
            };

            var validate = client.Deployments.ValidateAsync(resourceGroup, deploymentName, deployment, new CancellationToken()).Result;
            if (!validate.IsValid)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromObject(validate), null,
                     DefaultErrorCodes.DefaultErrorCode, $"Azure:{validate.Error.Message} Details:{validate.Error.Details}");
            }

            var deploymentItem = client.Deployments.CreateOrUpdateAsync(resourceGroup, deploymentName, deployment, new CancellationToken()).Result;
            return new ActionResponse(ActionStatus.Success, deploymentItem);

        }
    }
}