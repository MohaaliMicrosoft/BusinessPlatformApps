using System.ComponentModel.Composition;
using System.Dynamic;
using System.Net.Http;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.ErrorCode;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.DatabaseActions
{
    [Export(typeof(IAction))]
    public class ValidateAzureSqlExists : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
          

            string server = request.Message["SqlCredentials"].SelectToken("Server")?.ToString();
            string user = request.Message["SqlCredentials"].SelectToken("User")?.ToString();
            string password = request.Message["SqlCredentials"].SelectToken("Password")?.ToString();
            var database = request.Message["SqlCredentials"].SelectToken("Database")?.ToString();


            AzureHttpClient httpClient = new AzureHttpClient(token, subscription);
            dynamic payload = new ExpandoObject();
            payload.name = server.Replace(".database.windows.net", "");
            payload.type = "Microsoft.Sql/servers";


            HttpResponseMessage response = httpClient.ExecuteWithSubscriptionAsync(HttpMethod.Post, $"/providers/Microsoft.Sql/checkNameAvailability", "2014-04-01-preview", 
                JsonUtility.GetJsonStringFromObject(payload)).Result;
            string content = response.Content.ReadAsStringAsync().Result;
            var json =  JsonUtility.GetJObjectFromJsonString(content);
            bool isAvailable = json["available"].ToString().EqualsIgnoreCase("True");
            string reason = json["reason"].ToString();
            string message = json["message"].ToString();

            if (isAvailable)
            {
                return new ActionResponse(ActionStatus.Success, "");
            }
            else
            {
                // Handle basic errorcases here
                return new ActionResponse(ActionStatus.FailureExpected, json, null, SqlErrorCodes.DatabaseAlreadyExists);
            }
        }
    }
}
