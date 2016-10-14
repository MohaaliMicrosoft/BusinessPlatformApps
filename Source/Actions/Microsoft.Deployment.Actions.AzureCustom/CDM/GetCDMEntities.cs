using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bpst.Actions.AzureActions.CDM
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
