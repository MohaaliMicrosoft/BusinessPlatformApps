using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Bpst.Actions.SalesforceActions.Helpers;
using Microsoft.Bpst.Actions.SalesforceActions.MappingHelpers;
using Microsoft.Bpst.Actions.SalesforceActions.Models;
using Microsoft.Bpst.Actions.SalesforceActions.SalesforceSOAP;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Helpers;
using Newtonsoft.Json;

namespace Microsoft.Bpst.Actions.SalesforceActions
{
    [Export(typeof(IAction))]
    public class SalesforceSqlArtefacts : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string schema = "dbo";
            string connString = request.Message["SqlConnectionString"][0].ToString();

            string objectMetadata = request.Message["Objects"].ToString();
            List<DescribeSObjectResult> metadataList = JsonConvert.DeserializeObject(objectMetadata, typeof(List<DescribeSObjectResult>)) as List<DescribeSObjectResult>;
            List<Tuple<string, List<ADFField>>> adfFields = new List<Tuple<string, List<ADFField>>>();

            foreach (var obj in metadataList)
            {
                var simpleMetadata = ExtractSimpleMetadata(obj);

                adfFields.Add(new Tuple<string, List<ADFField>>(obj.name, simpleMetadata));

                CreateSqlTableAndTableType(simpleMetadata, obj.fields, schema, obj.name, connString);
                CreateStoredProcedure(simpleMetadata, string.Concat("spMerge", obj.name), schema, string.Concat(obj.name, "Type"), obj.name, connString);
            }

            dynamic resp = new ExpandoObject();
            resp.ADFPipelineJsonData = new ExpandoObject();
            resp.ADFPipelineJsonData.fields = adfFields;

            return new ActionResponse(ActionStatus.Success, resp);
        }

        public List<ADFField> ExtractSimpleMetadata(DescribeSObjectResult sfobject)
        {
            List<ADFField> simpleFields = new List<ADFField>();
            List<string> userFieldsThatAreNotSupportedByADF = new List<string>();

            userFieldsThatAreNotSupportedByADF.Add("MediumPhotoUrl");
            userFieldsThatAreNotSupportedByADF.Add("UserPreferencesHideBiggerPhotoCallout");
            userFieldsThatAreNotSupportedByADF.Add("UserPreferencesHideSfxWelcomeMat");
            userFieldsThatAreNotSupportedByADF.Add("UserPreferencesHideLightningMigrationModal");
            userFieldsThatAreNotSupportedByADF.Add("UserPreferencesHideEndUserOnboardingAssistantModal");
            userFieldsThatAreNotSupportedByADF.Add("UserPreferencesPreviewLightning");

            foreach (var field in sfobject.fields)
            {
                // check to go around ADF unsupported fields
                if (field.type != fieldType.address && !userFieldsThatAreNotSupportedByADF.Contains(field.name))
                {
                    var newField = new ADFField();

                    newField.name = field.name;
                    newField.type = field.soapType.ToString().Contains("xsd") ? field.soapType.ToString().Replace("xsd", string.Empty) : field.soapType.ToString();
                    newField.type = (newField.type == "tnsID" || newField.type == "tnsaddress") ? "string" : newField.type;

                    simpleFields.Add(newField);
                }
            }

            return simpleFields;
        }

        public void CreateSqlTableAndTableType(List<ADFField> fields, SalesforceSOAP.Field[] sfFields, string schemaName, string tableName, string connString)
        {
            StringBuilder sb = new StringBuilder();

            string createTable = string.Format("CREATE TABLE [{0}].[{1}](", schemaName, tableName);
            string createTableType = string.Format("CREATE TYPE [{0}].[{1}Type] AS TABLE(", schemaName, tableName);

            sb.AppendLine(createTable);

            foreach (var field in fields)
            {
                string sqlType;
                AdfToSqlMappingHelper.TypeMappings.TryGetValue(field.type.ToString(), out sqlType);
                if (!string.IsNullOrEmpty(sqlType) && sqlType == "nvarchar")
                {
                    int nvarcharSize = sfFields.First(e => e.name == field.name).length;

                    string size = string.Empty;

                    if (nvarcharSize > 8000)
                    {
                        size = "max";
                    }
                    if (nvarcharSize == 0)
                    {
                        size = "255";
                    }

                    sb.AppendLine(string.Format("[{0}] [{1}]({2}) NULL,",
                        field.name,
                        string.IsNullOrEmpty(sqlType) ? field.type.ToString() : sqlType,
                        !string.IsNullOrEmpty(size) ? size : nvarcharSize.ToString()));
                }
                else
                {
                    sb.AppendLine(string.Format("[{0}] [{1}] NULL,", field.name, string.IsNullOrEmpty(sqlType) ? field.type.ToString() : sqlType));
                }
            }

            sb.Remove(sb.Length - 3, 1);
            sb.AppendLine(")");

            SqlUtility.InvokeSqlCommand(connString, sb.ToString(), null);

            sb.Replace(createTable, createTableType);

            SqlUtility.InvokeSqlCommand(connString, sb.ToString(), null);
        }

        public void CreateStoredProcedure(List<ADFField> fields, string sprocName, string schemaName, string tableTypeName, string targetTableName, string connString)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("CREATE procedure [{0}].[{1}] @{2} [{0}].[{3}] READONLY as BEGIN", schemaName, sprocName, targetTableName, tableTypeName));
            sb.AppendLine(string.Format("MERGE [{0}].[{1}] AS TARGET \r\nUSING\r\n(SELECT", schemaName, targetTableName));

            foreach (var field in fields)
            {
                sb.AppendLine(string.Format("[{0}],", field.name));
            }

            sb.Remove(sb.Length - 3, 1);

            sb.AppendLine(string.Format("FROM @{0}\r\n) AS SOURCE\r\n ON SOURCE.ID = TARGET.ID \r\n WHEN MATCHED AND source.[LastModifiedDate] > target.[LastModifiedDate] THEN", targetTableName));

            sb.AppendLine("UPDATE \r\n SET");

            foreach (var field in fields)
            {
                sb.AppendLine(string.Format("TARGET.[{0}] = SOURCE.[{0}],", field.name));
            }

            sb.Remove(sb.Length - 3, 1);

            sb.AppendLine("WHEN NOT MATCHED BY TARGET THEN \r\nINSERT(");

            foreach (var field in fields)
            {
                sb.AppendLine(string.Format("[{0}],", field.name));
            }

            sb.Remove(sb.Length - 3, 1);
            sb.Append(")\r\n VALUES (");

            foreach (var field in fields)
            {
                sb.AppendLine(string.Format("SOURCE.[{0}],", field.name));
            }

            sb.Remove(sb.Length - 3, 1);
            sb.Append(");");

            var containsDelete = fields.Select(p => p.name == "IsDeleted");

            if (containsDelete.Contains(true))
            {
                sb.AppendLine($"DELETE FROM [{schemaName}].[{targetTableName}]");
                sb.AppendLine("WHERE IsDeleted = 1");
            }
            sb.AppendLine(@"END");

            SqlUtility.InvokeSqlCommand(connString, sb.ToString(), null);
        }
    }
}
