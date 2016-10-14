using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.ErrorCode;
using Microsoft.Bpst.Shared.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bpst.Actions.DatabaseActions
{
    [Export(typeof(IAction))]
    public class SetConfigValueInSql : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            // Provided by the json 
            var sqlIndex = int.Parse(request.Message["SqlServerIndex"].ToString());
            string configTable = request.Message["SqlConfigTable"].ToString();


            // Provided by thge user including the messages below
            string connectionString = request.Message["SqlConnectionString"][sqlIndex].ToString();
                // Must specify Initial Catalog

            // Get list of settings to deploy;
            JToken listGroup = request.Message.SelectToken("SqlGroup");
            JToken listSubgroup = request.Message.SelectToken("SqlSubGroup");
            JToken listConfigEntryName = request.Message.SelectToken("SqlEntryName");
            JToken listConfigEntryValue = request.Message.SelectToken("SqlEntryValue");

            if (listGroup == null || listSubgroup == null || listConfigEntryName == null || listConfigEntryValue == null)
            {
                return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject(),null, DefaultErrorCodes.DefaultErrorCode, 
                    "Configuration value properties not found");
            }

            if (listGroup.Type != JTokenType.Array || listSubgroup.Type != JTokenType.Array ||
                listConfigEntryName.Type != JTokenType.Array || listConfigEntryValue.Type != JTokenType.Array)
            {
                return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject(), null, DefaultErrorCodes.DefaultErrorCode, "Configuration is invalid");
            }


            for (int i = 0; i < listGroup.Count(); i++)
            {
                string group = request.Message["SqlGroup"][i].ToString();
                string subgroup = request.Message["SqlSubGroup"][i].ToString();
                string configEntryName = request.Message["SqlEntryName"][i].ToString();
                string configEntryValue = request.Message["SqlEntryValue"][i].ToString();

                string query = string.Format(queryTemplate, configTable, group, subgroup, configEntryName,
                    configEntryValue);

                SqlUtility.InvokeSqlCommand(connectionString, query, null);
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }

        private const string queryTemplate = @"MERGE {0} AS t  
                                           USING ( VALUES('{1}', '{2}', '{3}', '{4}') ) AS s(configuration_group, configuration_subgroup, [name], [value])
                                           ON t.configuration_group=s.configuration_group AND t.configuration_subgroup=s.configuration_subgroup AND t.[name]=s.[name]
                                           WHEN matched THEN
                                               UPDATE SET [value]=s.[value]
                                           WHEN NOT matched THEN
                                               INSERT (configuration_group, configuration_subgroup, [name], [value]) VALUES (s.configuration_group, s.configuration_subgroup, s.[name], s.[value]);";

    }
}