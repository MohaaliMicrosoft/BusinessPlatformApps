using System.ComponentModel.Composition;
using System.IO;
using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

//NOT NEEDED
namespace Microsoft.Deployment.Actions.AzureCustom.Twitter
{
    [Export(typeof(IAction))]
    public class DeployHistoricalTwitterFunction : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            var deploymentName = request.Message["DeploymentName"].ToString();
            var location = request.Message["SelectedLocation"][0]["Name"].ToString();
            var sitename = request.Message["SiteName"][0].ToString();
            var functionAppHostingPlan = request.Message["FunctionHostingPlan"][0].ToString();

            var param = new AzureArmParameterGenerator();
            param.AddStringParam("storageaccountname", "solutiontemplate" + Path.GetRandomFileName().Replace(".", "").Substring(0, 8));
            param.AddStringParam("sitename", sitename);
            param.AddStringParam("AppHostingPlan", functionAppHostingPlan);
            param.AddStringParam("resourcegroup", resourceGroup);
            param.AddStringParam("subscription", subscription);

            var armTemplate = JsonUtility.GetJObjectFromJsonString(System.IO.File.ReadAllText(Path.Combine(request.TemplatePath, "Service/AzureArm/function.json")));
            var armParamTemplate = JsonUtility.GetJObjectFromObject(param.GetDynamicObject());
            armTemplate.Remove("parameters");
            armTemplate.Add("parameters", armParamTemplate["parameters"]);

            SubscriptionCloudCredentials creds = new TokenCloudCredentials(subscription, token);
            Microsoft.Azure.Management.Resources.ResourceManagementClient client = new ResourceManagementClient(creds);



            return new ActionResponse(ActionStatus.Success);
        }
    }
}