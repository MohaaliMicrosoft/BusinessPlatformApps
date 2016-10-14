using System;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bpst.Actions.CustomActions.SAP
{
    [Export(typeof(IAction))]
    public class WriteSAPJson : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var rowBatchSize = request.Message["RowBatchSize"][0].ToString();
            var sapClient = request.Message["SapClient"][0].ToString();
            var sapHost = request.Message["SapHost"][0].ToString();
            var sapLanguage = request.Message["SapLanguage"][0].ToString();
            var sapRouterString = request.Message["SapRouterString"][0].ToString();
            var sapSystemId = request.Message["SapSystemId"][0].ToString();
            var sapSystemNumber = request.Message["SapSystemNumber"][0].ToString();
            var sqlConnectionString = request.Message["SqlConnectionString"][0].ToString();

            string jsonDestination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), JSON_PATH);
            (new FileInfo(jsonDestination)).Directory.Create();

            JObject config = new JObject(
                new JProperty("RowBatchSize", rowBatchSize),
                new JProperty("SapClient", sapClient),
                new JProperty("SapHost", sapHost),
                new JProperty("SapLanguage", sapLanguage),
                new JProperty("SapRouterString", sapRouterString),
                new JProperty("SapSystemId", sapSystemId),
                new JProperty("SapSystemNumber", sapSystemNumber),
                new JProperty("SqlConnectionString", sqlConnectionString)
            );

            using (StreamWriter file = File.CreateText(jsonDestination))
            {
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    config.WriteTo(writer);
                }
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }

        private const string JSON_PATH = @"Simplement, Inc\Solution Template AR\config.json";
    }
}