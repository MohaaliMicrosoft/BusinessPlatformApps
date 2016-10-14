using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using Microsoft.Bpst.Shared;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.AzureActions.AzureToken
{
    [Export(typeof(IAction))]
    public class GetManagementCoreUri : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string authBase = Constants.AzureAuthUri;

            Dictionary<string, string> message = new Dictionary<string, string>
            {
                {"client_id", Constants.MicrosoftClientId},
                {"prompt", "consent"},
                {"response_type", "code"},
                {"redirect_uri", Uri.EscapeDataString(request.SiteRootPath + Constants.WebsiteRedirectPath)},
                {"resource", Uri.EscapeDataString(Constants.AzureManagementCoreApi)}
            };

            StringBuilder builder = new StringBuilder();
            builder.Append(authBase);
            foreach (KeyValuePair<string, string> keyValuePair in message)
            {
                builder.Append(keyValuePair.Key + "=" + keyValuePair.Value);
                builder.Append("&");
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetJObjectFromStringValue(builder.ToString()));
        }
    }
}