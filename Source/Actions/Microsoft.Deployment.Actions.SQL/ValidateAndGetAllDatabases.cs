using System.ComponentModel.Composition;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Enums;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Deployment.Common.Model;

namespace Microsoft.Deployment.Actions.SQL
{
    [Export(typeof(IAction))]
    public class ValidateAndGetAllDatabases : BaseAction
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

            var response = SqlUtility.GetListOfDatabases(credentials, false);
            return response.Count == 0
                ? new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "NoDatabasesFound")
                : new ActionResponse(ActionStatus.Success, JsonUtility.CreateJObjectWithValueFromObject(response));
        }
    }
}