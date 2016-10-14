using System.ComponentModel.Composition;

using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Enums;
using Microsoft.Bpst.Shared.Helpers;
using Microsoft.Bpst.Shared.Model;

namespace Microsoft.Bpst.Actions.DatabaseActions
{
    [Export(typeof(IAction))]
    public class ValidateAndGetWritableDatabases : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string server = request.Message["SqlCredentials"]["Server"].ToString();
            string user = request.Message["SqlCredentials"].SelectToken("User")?.ToString();
            string password = request.Message["SqlCredentials"].SelectToken("Password")?.ToString();
            var auth = request.Message["SqlCredentials"]["AuthType"].ToString();

            SqlCredentials credentials = new SqlCredentials()
            {
                Server = server,
                Username = user,
                Password = password,
                Authentication = auth.EqualsIgnoreCase("Windows") ? SqlAuthentication.Windows : SqlAuthentication.SQL
            };

            var response = SqlUtility.GetListOfDatabases(credentials, true);
            return response.Count == 0
                ? new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "NoDatabasesFound")
                : new ActionResponse(ActionStatus.Success, JsonUtility.CreateJObjectWithValueFromObject(response));
        }
    }
}