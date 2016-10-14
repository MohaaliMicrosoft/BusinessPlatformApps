using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.Bpst.Actions.SalesforceActions.Helpers;
using Microsoft.Bpst.Actions.SalesforceActions.Models;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.ErrorCode;
using Microsoft.Bpst.Shared.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bpst.Actions.SalesforceActions
{
    [Export(typeof(IAction))]
    class ADFDeployDatasets : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            List<Task<ActionResponse>> task = new List<Task<ActionResponse>>();
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            string schema = "dbo";

            string postDeploymentPipelineType = request.Message["postDeploymentPipelineType"]?.ToString();
            string pipelineFrequency = request.Message["pipelineFrequency"][0]?.ToString();
            string pipelineInterval = request.Message["pipelineInterval"][0]?.ToString();
            string pipelineType = request.Message["pipelineType"][0]?.ToString();
            string pipelineStart = request.Message["pipelineStart"][0]?.ToString();
            string pipelineEnd = request.Message["pipelineEnd"][0]?.ToString();
            string connString = request.Message["SqlConnectionString"][0].ToString();

            string sfUsername = request.Message["Salesforce"][0].SelectToken("SalesforceUser")?.ToString();
            string sfPassword = request.Message["Salesforce"][0].SelectToken("SalesforcePassword")?.ToString();
            string sfToken = request.Message["Salesforce"][0].SelectToken("SalesforceToken")?.ToString();
            string sfEditionType = request.Message["SalesforceEditionType"][0]?.SelectToken("value").ToString();

            if (!string.IsNullOrWhiteSpace(postDeploymentPipelineType))
            {
                pipelineFrequency = request.Message["postDeploymentPipelineFrequency"][0]?.ToString();
                pipelineType = postDeploymentPipelineType;
                pipelineInterval = request.Message["postDeploymentPipelineInterval"][0]?.ToString();
            }

            var adfJsonData = request.Message["ADFPipelineJsonData"].ToString();
            adfJsonData = adfJsonData.Remove(0, 1);
            adfJsonData = adfJsonData.Remove(adfJsonData.Length - 1, 1);
            var sqlCreds = SqlUtility.GetSqlCredentialsFromConnectionString(connString);

            var obj = JsonConvert.DeserializeObject(adfJsonData, typeof(DeserializedADFPayload)) as DeserializedADFPayload;

            foreach (var o in obj.fields)
            {
                var deploymentName = string.Concat("ADFDataset", pipelineType, o.Item1);

                dynamic datasetParam = new AzureArmParameterGenerator();
                datasetParam.AddStringParam("dataFactoryName", resourceGroup + "SalesforceCopyFactory");
                datasetParam.AddStringParam("sqlServerName", sqlCreds.Server.Split('.')[0]);
                datasetParam.AddStringParam("sqlServerUsername", sqlCreds.Username);
                datasetParam.AddStringParam("targetDatabaseName", sqlCreds.Database);
                datasetParam.AddStringParam("targetSqlSchema", schema);
                datasetParam.AddStringParam("targetSqlTable", o.Item1);
                datasetParam.AddStringParam("salesforceUsername", sfUsername);
                datasetParam.AddStringParam("targetSalesforceTable", o.Item1);
                datasetParam.AddStringParam("pipelineName", o.Item1 + "CopyPipeline");
                datasetParam.AddStringParam("sqlWritableTypeName", o.Item1 + "Type");
                datasetParam.AddStringParam("sqlWriterStoredProcedureName", "spMerge" + o.Item1);
                datasetParam.AddStringParam("pipelineStartDate", pipelineStart);
                datasetParam.AddStringParam("pipelineEndDate", pipelineEnd);
                datasetParam.AddStringParam("sliceFrequency", pipelineFrequency);
                datasetParam.AddStringParam("sliceInterval", pipelineInterval);
                datasetParam.AddStringParam("pipelineType", pipelineType);
                datasetParam.AddParameter("salesforcePassword", "securestring", sfPassword);
                datasetParam.AddParameter("sqlServerPassword", "securestring", sqlCreds.Password);
                datasetParam.AddParameter("salesforceSecurityToken", "securestring", sfToken);

                var armTemplate = JsonUtility.GetJsonObjectFromJsonString(System.IO.File.ReadAllText(Path.Combine(request.TemplatePath, "Service/ADF/datasets.json")));
                var armParamTemplate = JsonUtility.GetJObjectFromObject(datasetParam.GetDynamicObject());

                armTemplate.Remove("parameters");
                armTemplate.Add("parameters", armParamTemplate["parameters"]);

                string tableFields = JsonConvert.SerializeObject(o.Item2);
                StringBuilder query = CreateQuery(o, tableFields);
                string stringTemplate = ReplaceTableFieldsAndQuery(tableFields, query, armTemplate);

                var creds = new TokenCloudCredentials(subscription, token);
                var client = new ResourceManagementClient(creds);

                var deployment = new Microsoft.Azure.Management.Resources.Models.Deployment()
                {
                    Properties = new DeploymentPropertiesExtended()
                    {
                        Template = stringTemplate,
                        Parameters = JsonUtility.GetEmptyJObject().ToString()
                    }
                };

                var validate = client.Deployments.ValidateAsync(resourceGroup, deploymentName, deployment, new CancellationToken()).Result;
                if (!validate.IsValid)
                {
                    return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromObject(validate), null,
                        DefaultErrorCodes.DefaultErrorCode, $"Azure:{validate.Error.Message} Details:{validate.Error.Details}");

                }

                task.Add(new Task<ActionResponse>(() =>
                {
                    var deploymentItem = client.Deployments.CreateOrUpdateAsync(resourceGroup, deploymentName, deployment, new CancellationToken()).Result;

                    var helper = new DeploymentHelper();
                    return helper.WaitForDeployment(resourceGroup, deploymentName, client);
                }));
            }

            foreach (var t in task)
            {
                t.Start();
            }

            Task.WaitAll(task.ToArray());

            foreach(var t in task)
            {
                if(t.Result.Status != ActionStatus.Success)
                {
                    return new ActionResponse(ActionStatus.Failure, t.Result.FriendlyErrorMessage);
                }
            }

            return new ActionResponse(ActionStatus.Success);
        }

        // Populate the template with the required fields and query
        private string ReplaceTableFieldsAndQuery(string tableFields, StringBuilder query, JObject armTemplate)
        {
            var stringTemplate = armTemplate.ToString();
            stringTemplate = stringTemplate.Replace("\"SQLTABLEDEFINITION\"", tableFields);
            stringTemplate = stringTemplate.Replace("\"SALESFORCETABLEDEFINITION\"", tableFields);
            stringTemplate = stringTemplate.Replace("\"SALESFORCEQUERY\"", query.ToString());
            return stringTemplate;
        }

        // Create the object specific query 
        private StringBuilder CreateQuery(Field o, string tableFields)
        {
            StringBuilder query = new StringBuilder();
            query.Append("\"$$Text.Format('SELECT");
            foreach (var item in o.Item2)
            {
                query.Append(" " + item.name + ",");
            }
            query.Remove(query.Length - 1, 1);

            if (tableFields.Contains("CreatedDate"))
            {
                if (tableFields.Contains("IsDeleted"))
                {
                    query.Append(" FROM " + o.Item1 + " WHERE (IsDeleted = 1 OR IsDeleted = 0) AND ((CreatedDate > {0:yyyy-MM-ddTHH:mm:sssZ} AND CreatedDate <= {1:yyyy-MM-ddTHH:mm:sssZ}) OR (LastModifiedDate > {0:yyyy-MM-ddTHH:mm:sssZ} AND LastModifiedDate <= {1:yyyy-MM-ddTHH:mm:sssZ}))', WindowStart,WindowEnd)\"");
                }
                else
                {
                    query.Append(" FROM " + o.Item1 + " WHERE (CreatedDate > {0:yyyy-MM-ddTHH:mm:sssZ} AND CreatedDate <= {1:yyyy-MM-ddTHH:mm:sssZ}) OR (LastModifiedDate > {0:yyyy-MM-ddTHH:mm:sssZ} AND LastModifiedDate <= {1:yyyy-MM-ddTHH:mm:sssZ})', WindowStart,WindowEnd)\"");
                }
            }
            else
            {
                if (tableFields.Contains("IsDeleted"))
                {
                    query.Append(" FROM " + o.Item1 + " WHERE (IsDeleted = 1 OR IsDeleted = 0) AND (LastModifiedDate > {0:yyyy-MM-ddTHH:mm:sssZ} AND LastModifiedDate <= {1:yyyy-MM-ddTHH:mm:sssZ})', WindowStart,WindowEnd)\"");
                }
                else
                {
                    query.Append(" FROM " + o.Item1 + " WHERE LastModifiedDate > {0:yyyy-MM-ddTHH:mm:sssZ} AND LastModifiedDate <= {1:yyyy-MM-ddTHH:mm:sssZ}', WindowStart,WindowEnd)\"");
                }
            }

            return query;
        }
    }
}