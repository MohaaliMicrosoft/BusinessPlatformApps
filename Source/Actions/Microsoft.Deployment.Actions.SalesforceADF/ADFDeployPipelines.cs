using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.Deployment.Actions.SalesforceADF.Helpers;
using Microsoft.Deployment.Actions.SalesforceADF.Models;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Actions.SalesforceADF
{
    [Export(typeof(IAction))]
    class ADFDeployPipelines : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            List<Task<ActionResponse>> task = new List<Task<ActionResponse>>();
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            string connString = request.Message["SqlConnectionString"][0].ToString();
            string schema = "dbo";

            string sfUsername = request.Message["Salesforce"][0].SelectToken("SalesforceUser")?.ToString();
            string sfPassword = request.Message["Salesforce"][0].SelectToken("SalesforcePassword")?.ToString();
            string sfToken = request.Message["Salesforce"][0].SelectToken("SalesforceToken")?.ToString();
            string sfEditionType = request.Message["SalesforceEditionType"][0]?.SelectToken("value").ToString();
            int concurrentConnections = SalesforceConcurrencyMapper.Limits.Where(p => p.Item1 == sfEditionType).FirstOrDefault().Item2;

            string postDeploymentPipelineType = request.Message["postDeploymentPipelineType"]?.ToString();

            string pipelineFrequency = request.Message["pipelineFrequency"][0]?.ToString();
            string pipelineInterval = request.Message["pipelineInterval"][0]?.ToString();
            string pipelineType = request.Message["pipelineType"][0]?.ToString();
            string pipelineStart = request.Message["pipelineStart"][0]?.ToString();
            string pipelineEnd = request.Message["pipelineEnd"][0]?.ToString();
            bool historicalOnly = Convert.ToBoolean(request.Message["historicalOnly"][0]?.ToString());

            string dataFactoryName = resourceGroup + "SalesforceCopyFactory";

            if (!string.IsNullOrWhiteSpace(postDeploymentPipelineType))
            {
                pipelineFrequency = request.Message["postDeploymentPipelineFrequency"][0]?.ToString();
                pipelineType = postDeploymentPipelineType;
                pipelineInterval = request.Message["postDeploymentPipelineInterval"][0]?.ToString();
                pipelineStart = DateTime.UtcNow.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                pipelineEnd = new DateTime(9999, 12, 31, 23, 59, 59).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            }

            if (string.IsNullOrWhiteSpace(pipelineStart))
            {
                pipelineStart = DateTime.UtcNow.AddYears(-3).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            }

            if (string.IsNullOrWhiteSpace(pipelineEnd))
            {
                pipelineEnd = DateTime.UtcNow.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            }

            var adfJsonData = request.Message["ADFPipelineJsonData"].ToString();
            adfJsonData = adfJsonData.Remove(0, 1);
            adfJsonData = adfJsonData.Remove(adfJsonData.Length - 1, 1);

            var obj = JsonConvert.DeserializeObject(adfJsonData, typeof(DeserializedADFPayload)) as DeserializedADFPayload;

            obj = ReorderObjects(obj);

            for (int i = 0; i < obj.fields.Count(); i++)
            {
                var o = obj.fields[i];

                string deploymentName = string.Concat("ADFPipeline", pipelineType, o.Item1);

                var sqlCreds = SqlUtility.GetSqlCredentialsFromConnectionString(connString);
                var param = new AzureArmParameterGenerator();
                param.AddStringParam("dataFactoryName", dataFactoryName);
                param.AddStringParam("sqlServerName", sqlCreds.Server.Split('.')[0]);
                param.AddStringParam("sqlServerUsername", sqlCreds.Username);
                param.AddStringParam("targetDatabaseName", sqlCreds.Database);
                param.AddStringParam("targetSqlSchema", schema);
                param.AddStringParam("targetSqlTable", o.Item1);
                param.AddStringParam("salesforceUsername", sfUsername);
                param.AddStringParam("targetSalesforceTable", o.Item1);
                param.AddStringParam("pipelineName", o.Item1 + "CopyPipeline");
                param.AddStringParam("sqlWritableTypeName", o.Item1 + "Type");
                param.AddStringParam("sqlWriterStoredProcedureName", "spMerge" + o.Item1);
                param.AddStringParam("pipelineStartDate", pipelineStart);
                param.AddStringParam("pipelineEndDate", pipelineEnd);
                param.AddStringParam("sliceFrequency", pipelineFrequency);
                param.AddStringParam("sliceInterval", pipelineInterval);
                param.AddStringParam("pipelineType", pipelineType);
                param.AddParameter("salesforcePassword", "securestring", sfPassword);
                param.AddParameter("sqlServerPassword", "securestring", sqlCreds.Password);
                param.AddParameter("salesforceSecurityToken", "securestring", sfToken);

                var armTemplate = JsonUtility.GetJsonObjectFromJsonString(System.IO.File.ReadAllText(Path.Combine(request.TemplatePath, "Service/ADF/pipeline.json")));
                var armParamTemplate = JsonUtility.GetJObjectFromObject(param.GetDynamicObject());

                armTemplate.Remove("parameters");
                armTemplate.Add("parameters", armParamTemplate["parameters"]);

                if (concurrentConnections < 25)
                {
                    if (i >= 1)
                    {
                        armTemplate = CreatePipelineDependency(pipelineType, obj, armTemplate, i - 1);
                    }
                }

                string tableFields = JsonConvert.SerializeObject(o.Item2);

                StringBuilder query;

                if (o.Item1 != "Opportunity" &&
                    o.Item1 != "Lead" &&
                    o.Item1 != "OpportunityLineItem" &&
                    pipelineType == "PreDeployment")
                {
                    query = CreateQuery(o, tableFields, true, pipelineStart, pipelineEnd);
                    armTemplate = CreateOneTimePipeline(armTemplate);
                }
                else
                {
                    query = CreateQuery(o, tableFields, false);
                }

                if (historicalOnly && pipelineType == "PostDeployment")
                {
                    armTemplate = this.PausePipeline(armTemplate);
                }

                string stringTemplate = ReplaceTableFieldsAndQuery(tableFields, query, armTemplate);

                SubscriptionCloudCredentials creds = new TokenCloudCredentials(subscription, token);
                ResourceManagementClient client = new ResourceManagementClient(creds);

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

            foreach (var t in task)
            {
                if (t.Result.Status != ActionStatus.Success)
                {
                    return new ActionResponse(ActionStatus.Failure, t.Result.FriendlyErrorMessage);
                }
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetJObjectFromStringValue(dataFactoryName));
        }

        // Reorder the objects so the scheduled pipelines
        // (Opportunity, Lead, OpportunityLineItem) are 
        // dependent on the scheduled ones
        private DeserializedADFPayload ReorderObjects(DeserializedADFPayload obj)
        {
            var opportunity = obj.fields.Where(o => o.Item1 == "Opportunity").FirstOrDefault();
            var leads = obj.fields.Where(o => o.Item1 == "Lead").FirstOrDefault();
            var items = obj.fields.Where(o => o.Item1 == "OpportunityLineItem").FirstOrDefault();

            var objects = obj.fields.ToList();
            objects.Remove(opportunity);
            objects.Remove(leads);
            objects.Remove(items);

            objects.Add(opportunity);
            objects.Add(leads);
            objects.Add(items);

            obj.fields = objects.ToArray();

            return obj;
        }

        //Pause post deployment pipeline
        private JObject PausePipeline(JObject armTemplate)
        {
            (armTemplate
            .SelectToken("resources")[0]
            .SelectToken("properties") as JObject).
            Add("isPaused","true");

            return armTemplate;
        }

        // Modify existing ARM template to create
        // a one time only pipeline
        public JObject CreateOneTimePipeline(JObject armTemplate)
        {
            (armTemplate
                   .SelectToken("resources")[0]
                   .SelectToken("properties")
                   .SelectToken("activities")[0] as JObject)
                   .Remove("scheduler");

            (armTemplate
                .SelectToken("resources")[0]
                .SelectToken("properties") as JObject)
                .Add("pipelineMode", "OneTime");

            // One time pipelines must have the same
            // start and end date
            (armTemplate
                .SelectToken("resources")[0]
                .SelectToken("properties") as JObject)
                .Remove("end");

            var start = armTemplate
                .SelectToken("resources")[0]
                .SelectToken("properties")
                .SelectToken("start").ToString();

            (armTemplate
                .SelectToken("resources")[0]
                .SelectToken("properties") as JObject)
                .Add("end", start);

            return armTemplate;
        }

        // Create dependencies between pipelines so
        // the concurrency limit for smaller Salesforce
        // instances is not broken
        private JObject CreatePipelineDependency(string pipelineType, DeserializedADFPayload obj, JObject armTemplate, int index)
        {
            var currentDataset = pipelineType+"_" + obj.fields[index + 1].Item1 + "_Output";
            var dataset = pipelineType + "_" + obj.fields[index].Item1 + "_Output";
            var dependency = JToken.Parse(string.Concat("{ \"name\": \"", dataset, "\"}"));

            if (currentDataset != "PreDeployment_Opportunity_Output")
            {
                (armTemplate
                .SelectToken("resources")[0]
                .SelectToken("properties")
                .SelectToken("activities")[0]
                .SelectToken("inputs") as JArray).
                Add(dependency);
            }

            return armTemplate;
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
        private StringBuilder CreateQuery(Field o, string tableFields, bool oneTimePipeline, string startTime = null, string endTime = null)
        {
            StringBuilder query = new StringBuilder();
            query.Append("\"$$Text.Format('SELECT");
            foreach (var item in o.Item2)
            {
                query.Append(" " + item.name + ",");
            }
            query.Remove(query.Length - 1, 1);

            if (oneTimePipeline)
            {
                if (tableFields.Contains("CreatedDate"))
                {
                    if (tableFields.Contains("IsDeleted"))
                    {
                        query.Append(" FROM " + o.Item1 + $" WHERE (IsDeleted = FALSE OR IsDeleted = TRUE) AND ((CreatedDate > {startTime} AND CreatedDate <= {endTime}) OR (LastModifiedDate > {startTime} AND LastModifiedDate <= {endTime}))')\"");
                    }
                    else
                    {
                        query.Append(" FROM " + o.Item1 + $" WHERE (CreatedDate > {startTime} AND CreatedDate <= {endTime}) OR (LastModifiedDate > {startTime} AND LastModifiedDate <= {endTime})')\"");
                    }
                }
                else
                {
                    if (tableFields.Contains("IsDeleted"))
                    {
                        query.Append(" FROM " + o.Item1 + $" WHERE (IsDeleted = FALSE OR IsDeleted = TRUE) AND (LastModifiedDate > {startTime} AND LastModifiedDate <=  {endTime})')\"");
                    }
                    else
                    {
                        query.Append(" FROM " + o.Item1 + $" WHERE LastModifiedDate > {startTime} AND LastModifiedDate <= {endTime}')\"");
                    }
                }
            }
            else
            {
                if (tableFields.Contains("CreatedDate"))
                {
                    if (tableFields.Contains("IsDeleted"))
                    {
                        query.Append(" FROM " + o.Item1 + " WHERE (IsDeleted = FALSE OR IsDeleted = TRUE) AND ((CreatedDate > {0:yyyy-MM-ddTHH:mm:sssZ} AND CreatedDate <= {1:yyyy-MM-ddTHH:mm:sssZ}) OR (LastModifiedDate > {0:yyyy-MM-ddTHH:mm:sssZ} AND LastModifiedDate <= {1:yyyy-MM-ddTHH:mm:sssZ}))', WindowStart,WindowEnd)\"");
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
                        query.Append(" FROM " + o.Item1 + " WHERE (IsDeleted = FALSE OR IsDeleted = TRUE) AND (LastModifiedDate > {0:yyyy-MM-ddTHH:mm:sssZ} AND LastModifiedDate <= {1:yyyy-MM-ddTHH:mm:sssZ})', WindowStart,WindowEnd)\"");
                    }
                    else
                    {
                        query.Append(" FROM " + o.Item1 + " WHERE LastModifiedDate > {0:yyyy-MM-ddTHH:mm:sssZ} AND LastModifiedDate <= {1:yyyy-MM-ddTHH:mm:sssZ}', WindowStart,WindowEnd)\"");
                    }
                }
            }

            return query;
        }
    }
}
