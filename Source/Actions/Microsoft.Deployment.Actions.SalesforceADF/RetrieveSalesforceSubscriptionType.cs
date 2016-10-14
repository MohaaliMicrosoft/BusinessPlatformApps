using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel;
using Microsoft.Bpst.Actions.SalesforceActions.SalesforceSOAP;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.SalesforceActions
{
    [Export(typeof(IAction))]
    public class RetrieveSalesforceSubscriptionType : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string salesforceType = string.Empty;

            string sessionId = request.Message["sessionId"]?.ToString();
            string serverUrl = request.Message["serverUrl"]?.ToString();
            string metadataUrl = request.Message["metadataServerUrl"]?.ToString();

            SessionHeader sheader = new SessionHeader();
            SoapClient binding = new SoapClient("Soap");
            binding = new SoapClient("Soap");

            binding.Endpoint.Address = new EndpointAddress(serverUrl);

            sheader.sessionId = sessionId;

            binding.Endpoint.ListenUri = new Uri(metadataUrl);

            QueryResult result;
            binding.query(sheader, null, null, null, null, "SELECT OrganizationType FROM Organization", out result);

            salesforceType =  result.records[0]?.Any.Where(p => p.Name == "sf:OrganizationType").FirstOrDefault()?.InnerText;

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetJObjectFromStringValue(salesforceType));
        }
    }
}
