using System.ComponentModel.Composition;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Enums;
using Microsoft.Bpst.Shared.Helpers;
using Microsoft.Bpst.Shared.Model;

namespace Microsoft.Bpst.Actions.DatabaseActions
{
    [Export(typeof(IAction))]
    public class GetSqlConnectionString : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string server = request.Message["SqlCredentials"].SelectToken("Server")?.ToString();
            string user = request.Message["SqlCredentials"].SelectToken("User")?.ToString();
            string password = request.Message["SqlCredentials"].SelectToken("Password")?.ToString();
            var auth = request.Message["SqlCredentials"].SelectToken("AuthType")?.ToString();
            var database = request.Message["SqlCredentials"].SelectToken("Database")?.ToString();

            SqlCredentials credentials = new SqlCredentials()
            {
                Server = server,
                Username = user,
                Password = password,
                Authentication = auth.EqualsIgnoreCase("Windows") ? SqlAuthentication.Windows : SqlAuthentication.SQL,
                Database = string.IsNullOrEmpty(database) ? "master" : database
            };

            var response = SqlUtility.GetConnectionString(credentials);
            return new ActionResponse(ActionStatus.Success, JsonUtility.CreateJObjectWithValueFromObject(response));
        }
    }
}
