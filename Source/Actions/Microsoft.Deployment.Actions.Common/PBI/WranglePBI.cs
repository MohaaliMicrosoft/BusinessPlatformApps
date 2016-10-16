using System.ComponentModel.Composition;
using System.IO;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.Common.PBI
{
    [Export(typeof(IAction))]
    public class WranglePBI : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            int sqlIndex = 0;
            if (request.Message.SelectToken("SqlServerIndex") != null)
            {
                sqlIndex = int.Parse(request.Message["SqlServerIndex"].ToString());
            }

            var filename = request.Message["FileName"].ToString();
            string connectionString = request.Message["SqlConnectionString"][sqlIndex].ToString();

            var templateFullPath = request.TemplatePath + $"/service/PowerBI/{filename}";
            var tempfileName = Path.GetRandomFileName();
            var templateTempFullPath = request.TemplatePath + $"/service/PowerBI/Temp/{tempfileName}/{filename}";
            Directory.CreateDirectory(request.TemplatePath + $"/service/PowerBI/Temp/{tempfileName}");

            var creds = SqlUtility.GetSqlCredentialsFromConnectionString(connectionString);

            using (PBIXUtils wrangler = new PBIXUtils(templateFullPath, templateTempFullPath))
            {
                wrangler.ReplaceKnownVariableinMashup("STSqlServer", creds.Server);
                wrangler.ReplaceKnownVariableinMashup("STSqlDatabase", creds.Database);
            }

            string serverPath = request.SiteTemplatePath + $"/service/PowerBI/Temp/{tempfileName}/{filename}";
            return new ActionResponse(ActionStatus.Success, JsonUtility.GetJObjectFromStringValue(serverPath));
        }
    }
}