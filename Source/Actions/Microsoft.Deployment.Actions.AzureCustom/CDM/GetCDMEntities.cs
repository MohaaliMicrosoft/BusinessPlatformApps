using System.ComponentModel.Composition;
using System.Net.Http;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.AzureCustom.CDM
{
    [Export(typeof(IAction))]
    public class GetCDMEntities : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][1]["access_token"].ToString();
            AzureHttpClient client = new AzureHttpClient(token);
            var response = client.ExecuteGenericRequestWithHeaderAsync(HttpMethod.Get, "providers/Microsoft.PowerApps/entities", "");
            
            return new ActionResponse(ActionStatus.Success, response);
        }
    }
}
