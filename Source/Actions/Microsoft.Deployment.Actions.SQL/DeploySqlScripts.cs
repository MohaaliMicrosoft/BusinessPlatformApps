using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.SQL
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