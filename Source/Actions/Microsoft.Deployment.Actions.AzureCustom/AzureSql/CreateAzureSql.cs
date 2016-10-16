using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Enums;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Deployment.Common.Model;

namespace Microsoft.Deployment.Actions.AzureCustom.AzureSql
{
    [Export(typeof(IAction))]
    public class CreateAzureSql : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();

            string server = request.Message["SqlCredentials"].SelectToken("Server")?.ToString();
            string user = request.Message["SqlCredentials"].SelectToken("User")?.ToString();
            string password = request.Message["SqlCredentials"].SelectToken("Password")?.ToString();
            var database = request.Message["SqlCredentials"].SelectToken("Database")?.ToString();

            string serverWithoutExtension = server.Replace(".database.windows.net", string.Empty);

            var param = new AzureArmParameterGenerator();
            param.AddStringParam("SqlServerName", serverWithoutExtension);
            param.AddStringParam("SqlDatabaseName", database);
            param.AddStringParam("Username", user);
            param.AddParameter("Password", "securestring", password);            

            var armTemplate = JsonUtility.GetJObjectFromJsonString(System.IO.File.ReadAllText(Path.Combine(request.TemplateRootPath, "Common/Service/Arm/sqlserveranddatabase.json")));
            var armParamTemplate = JsonUtility.GetJObjectFromObject(param.GetDynamicObject());
            armTemplate.Remove("parameters");
            armTemplate.Add("parameters", armParamTemplate["parameters"]);

            SubscriptionCloudCredentials creds = new TokenCloudCredentials(subscription, token);
            ResourceManagementClient client = new ResourceManagementClient(creds);

            var deployment = new Microsoft.Azure.Management.Resources.Models.Deployment()
            {
                Properties = new DeploymentPropertiesExtended()
                {
                    Template = armTemplate.ToString(),
                    Parameters = JsonUtility.GetEmptyJObject().ToString()
                }
            };

            string deploymentName = "SqlDatabaseDeployment";

            var validate = client.Deployments.ValidateAsync(resourceGroup, deploymentName, deployment, new CancellationToken()).Result;
            if (!validate.IsValid)
            {
                return new ActionResponse(
                    ActionStatus.Failure,
                    JsonUtility.GetJObjectFromObject(validate),
                    null,
                    DefaultErrorCodes.DefaultErrorCode,
                    $"Azure:{validate.Error.Message} Details:{validate.Error.Details}");
            }

            var deploymentItem = client.Deployments.CreateOrUpdateAsync(resourceGroup, deploymentName, deployment, new CancellationToken()).Result;

            // Wait for deployment
            while (true)
            {
                Thread.Sleep(5000);
                var status = client.Deployments.GetAsync(resourceGroup, deploymentName, new CancellationToken()).Result;
                var operations = client.DeploymentOperations.ListAsync(resourceGroup, deploymentName, new DeploymentOperationsListParameters()).Result;
                var provisioningState = status.Deployment.Properties.ProvisioningState;

                if (provisioningState == "Accepted" || provisioningState == "Running")
                    continue;

                if (provisioningState == "Succeeded")
                    break;

                var operation = operations.Operations.First(p => p.Properties.ProvisioningState == ProvisioningState.Failed);
                var operationFailed = client.DeploymentOperations.GetAsync(resourceGroup, deploymentName, operation.OperationId).Result;

                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromObject(operationFailed), null, DefaultErrorCodes.DefaultErrorCode, operationFailed.Operation.Properties.StatusMessage);                
            }

            SqlCredentials credentials = new SqlCredentials()
            {
                Server = server,
                Username = user,
                Password = password,
                Authentication = SqlAuthentication.SQL,
                Database = database
            };

            var connectionStringResponse = SqlUtility.GetConnectionString(credentials);
            return new ActionResponse(ActionStatus.Success, JsonUtility.CreateJObjectWithValueFromObject(connectionStringResponse));
        }
    }
}