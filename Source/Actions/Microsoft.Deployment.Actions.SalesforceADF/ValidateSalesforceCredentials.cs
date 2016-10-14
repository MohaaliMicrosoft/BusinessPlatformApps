using System;
using System.ComponentModel.Composition;
using System.Net;
using Microsoft.Deployment.Common.Actions;

namespace Microsoft.Deployment.Actions.SalesforceADF
{
    [Export(typeof(IAction))]
    public class ValidateSalesforceCredentials : BaseAction
    {
        public string sandboxUrl = "https://test.salesforce.com/";

        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string sfUsername = request.Message["Salesforce"][0].SelectToken("SalesforceUser")?.ToString();
            string sfPassword = request.Message["Salesforce"][0].SelectToken("SalesforcePassword")?.ToString();
            string sfToken = request.Message["Salesforce"][0].SelectToken("SalesforceToken")?.ToString();
            string sfTestUrl = request.Message["Salesforce"][0].SelectToken("SalesforceUrl")?.ToString();

            SoapClient binding = new SoapClient("Soap");
            LoginResult lr;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = true;

            binding.ClientCredentials.UserName.UserName = sfUsername;
            binding.ClientCredentials.UserName.Password = sfPassword;

            if (!string.IsNullOrEmpty(sfTestUrl) && sfTestUrl == this.sandboxUrl)
            {
                binding.Endpoint.Address = new System.ServiceModel.EndpointAddress(binding.Endpoint.Address.ToString().Replace("login", "test"));
            }

            try
            {
                lr =
                   binding.login(null, null, sfUsername,
                   string.Concat(sfPassword, sfToken));
            }
            catch(Exception e)
            {
                return new ActionResponse(ActionStatus.Failure, e);
            }

            return new ActionResponse(ActionStatus.Success, lr);
        }
    }
}
