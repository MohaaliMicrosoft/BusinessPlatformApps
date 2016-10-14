using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.Bpst.Actions.SalesforceActions.Helpers;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.ErrorCode;
using Microsoft.Bpst.Shared.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bpst.Actions.SalesforceActions
{
    [Export(typeof(IAction))]
    class ADFDeployLinkedServicesAndSqlCustomizations : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            string sfUsername = request.Message["Salesforce"][0].SelectToken("SalesforceUser")?.ToString();
            string sfPassword = request.Message["Salesforce"][0].SelectToken("SalesforcePassword")?.ToString();
            string sfToken = request.Message["Salesforce"][0].SelectToken("SalesforceToken")?.ToString();
            string fullServerUrl = request.Message["SalesforceBaseUrl"][0]?.ToString();
            string connString = request.Message["SqlConnectionString"][0].ToString();
            string fiscalMonth = request.Message["fiscalMonth"]?[0]?.ToString();
            string actuals = request.Message["actuals"]?[0]?.ToString();
            string emails = request.Message["EmailAddresses"]?[0]?.ToString();

            string baseUrl = string.Empty;
            if(!string.IsNullOrEmpty(fullServerUrl))
            {
                var uri = new Uri(fullServerUrl);
                baseUrl = uri.Scheme + "://" + uri.Host + "/";
            }

            this.InsertCustomizations(connString, fiscalMonth, actuals, baseUrl);

            string dataFactoryName = resourceGroup + "SalesforceCopyFactory";
            var param = new AzureArmParameterGenerator();
            var sqlCreds = SqlUtility.GetSqlCredentialsFromConnectionString(connString);
            param.AddStringParam("dataFactoryName", dataFactoryName);
            param.AddStringParam("sqlServerName", sqlCreds.Server.Split('.')[0]);
            param.AddStringParam("sqlServerUsername", sqlCreds.Username);
            param.AddStringParam("targetDatabaseName", sqlCreds.Database);
            param.AddStringParam("salesforceUsername", sfUsername);
            param.AddStringParam("subscriptionId", subscription);
            param.AddParameter("salesforcePassword", "securestring", sfPassword);
            param.AddParameter("sqlServerPassword", "securestring", sqlCreds.Password);
            param.AddParameter("salesforceSecurityToken", "securestring", sfToken);

            var armTemplate = JsonUtility.GetJsonObjectFromJsonString(System.IO.File.ReadAllText(Path.Combine(request.TemplatePath, "Service/ADF/linkedServices.json")));
            var armParamTemplate = JsonUtility.GetJObjectFromObject(param.GetDynamicObject());

            armTemplate.Remove("parameters");
            armTemplate.Add("parameters", armParamTemplate["parameters"]);

            if (string.IsNullOrEmpty(emails))
            {
                (armTemplate
                    .SelectToken("resources")[0]
                    .SelectToken("resources") as JArray)
                    .RemoveAt(2);
            }
            else
            {
                var addresses = emails.Split(',');
                List<string> adr = new List<string>();

                foreach (var address in addresses)
                {
                    adr.Add(address);
                }

                var stringTemplate = armTemplate.ToString();

                stringTemplate = stringTemplate.Replace("\"EMAILS\"", JsonConvert.SerializeObject(adr));
                armTemplate = JsonUtility.GetJObjectFromJsonString(stringTemplate);
            }

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

            var factoryIdenity = new ResourceIdentity
            {
                ResourceProviderApiVersion = "2015-10-01",
                ResourceName = dataFactoryName,
                ResourceProviderNamespace = "Microsoft.DataFactory",
                ResourceType = "datafactories"
            };

            var factory = client.Resources.CheckExistence(resourceGroup, factoryIdenity);

            if (factory.Exists)
            {
                client.Resources.Delete(resourceGroup, factoryIdenity);
            }

            string deploymentName = "SalesforceCopyFactory-linkedServices";

            var validate = client.Deployments.ValidateAsync(resourceGroup, deploymentName, deployment, new CancellationToken()).Result;
            if (!validate.IsValid)
            {
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromObject(validate), null,
                    DefaultErrorCodes.DefaultErrorCode, $"Azure:{validate.Error.Message} Details:{validate.Error.Details}");
            }

            var deploymentItem = client.Deployments.CreateOrUpdateAsync(resourceGroup, deploymentName, deployment, new CancellationToken()).Result;

            var helper = new DeploymentHelper();

            return helper.WaitForDeployment(resourceGroup, deploymentName, client);
        }


        public void InsertCustomizations(string connString, string fiscalMonth, string actuals, string baseUrl)
        {
            int monthNumber = DateTime.ParseExact(fiscalMonth, "MMMM", CultureInfo.InvariantCulture).Month;
            bool actual = actuals != "Closed opportunities";

            DeleteConfigurationValues(connString, "SolutionTemplate", "SalesManagement", "Source");
            DeleteConfigurationValues(connString, "SolutionTemplate", "SalesManagement", "FiscalMonthStart");
            DeleteConfigurationValues(connString, "data", "actual_sales", "enabled");
            DeleteConfigurationValues(connString, "SolutionTemplate", "SalesManagement", "BaseURL");

            AddConfigurationValues(connString, "SolutionTemplate", "SalesManagement", "FiscalMonthStart", monthNumber.ToString(CultureInfo.InvariantCulture));
            AddConfigurationValues(connString, "data", "actual_sales", "enabled", Convert.ToInt32(actual).ToString(CultureInfo.InvariantCulture));
            AddConfigurationValues(connString, "SolutionTemplate", "SalesManagement", "BaseURL", baseUrl);
            AddConfigurationValues(connString, "SolutionTemplate", "SalesManagement", "Source", "Salesforce");
        }

        public void AddConfigurationValues(string connString, string group, string subgroup,
                                        string name, string value, bool visible = true)
        {
            var configValues = new Dictionary<string, string>();
            configValues.Add("arg1", group);
            configValues.Add("arg2", subgroup);
            configValues.Add("arg3", name);
            configValues.Add("arg4", value);
            configValues.Add("arg5", Convert.ToInt32(visible).ToString(CultureInfo.InvariantCulture));

            SqlUtility.InvokeSqlCommand(connString, SqlConfigQueries.configQuery, configValues);
        }

        public void DeleteConfigurationValues(string connString, string group, string subgroup, string name)
        {
            var configValues = new Dictionary<string, string> { { "arg1", @group }, { "arg2", subgroup }, { "arg3", name } };
            SqlUtility.InvokeSqlCommand(connString, SqlConfigQueries.deleteConfigQuery, configValues);
        }
    }
}