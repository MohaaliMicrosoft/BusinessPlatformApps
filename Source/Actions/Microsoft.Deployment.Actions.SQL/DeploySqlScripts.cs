using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.DatabaseActions
{
    [Export(typeof(IAction))]
    public class DeploySQLScripts : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var sqlIndex = int.Parse(request.Message["SqlServerIndex"].ToString());
            var sqlScriptsFolder = request.Message["SqlScriptsFolder"].ToString();

            string connectionString = request.Message["SqlConnectionString"][sqlIndex].ToString();
 
            var files = Directory.EnumerateFiles(Path.Combine(request.TemplatePath,sqlScriptsFolder)).ToList();
            files.ForEach(f=>SqlUtility.InvokeSqlCommand(connectionString, File.ReadAllText(f), new Dictionary<string, string>()));
            return new ActionResponse(ActionStatus.Success,JsonUtility.GetEmptyJObject());
        }
    }
}