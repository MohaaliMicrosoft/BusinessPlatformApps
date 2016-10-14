using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using Microsoft.Deployment.Common.Actions;

namespace Microsoft.Deployment.Actions.SalesforceADF
{
    [Export(typeof(IAction))]
    class SalesforceGetObjectMetadata : BaseAction
    {
        private string sandboxUrl = "https://test.salesforce.com/";

        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string sfUsername = request.Message["Salesforce"][0].SelectToken("SalesforceUser")?.ToString();
            string sfPassword = request.Message["Salesforce"][0].SelectToken("SalesforcePassword")?.ToString();
            string sfToken = request.Message["Salesforce"][0].SelectToken("SalesforceToken")?.ToString();
            string objects = request.Message["Salesforce"][0].SelectToken("ObjectTables")?.ToString();
            string sfTestUrl = request.Message["Salesforce"][0].SelectToken("SalesforceUrl")?.ToString();

            List<string> sfObjects = objects.Split(',').ToList();

            SoapClient binding = new SoapClient("Soap");

            if (!string.IsNullOrEmpty(sfTestUrl) && sfTestUrl == this.sandboxUrl)
            {
                binding.Endpoint.Address = new System.ServiceModel.EndpointAddress(binding.Endpoint.Address.ToString().Replace("login", "test"));
            }

            LoginResult lr;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = true;

            binding.ClientCredentials.UserName.UserName = sfUsername;
            binding.ClientCredentials.UserName.Password = sfPassword;

            lr =
               binding.login(null, null, //LoginScopeHeader
               sfUsername,
               string.Concat(sfPassword, sfToken));

            dynamic metadata = new ExpandoObject();

            binding = new SoapClient("Soap");
            metadata.Objects = new List<DescribeSObjectResult>();
            SessionHeader sheader = new SessionHeader();
            BasicHttpBinding bind = new BasicHttpBinding();
            bind = (BasicHttpBinding)binding.Endpoint.Binding;
            bind.MaxReceivedMessageSize = 2147483647;
            bind.MaxBufferPoolSize = 2147483647;
            bind.MaxBufferSize = 2147483647;
            bind.CloseTimeout = new TimeSpan(0, 0, 5, 0);
            bind.OpenTimeout = new TimeSpan(0, 0, 5, 0);
            bind.ReaderQuotas.MaxArrayLength = 2147483647;
            bind.ReaderQuotas.MaxDepth = 2147483647;
            bind.ReaderQuotas.MaxNameTableCharCount = 2147483647;
            bind.ReaderQuotas.MaxStringContentLength = 2147483647;
            bind.ReaderQuotas.MaxBytesPerRead = 2147483647;
            bind.ReaderQuotas.MaxNameTableCharCount = 2147483647;

            binding.Endpoint.Binding = bind;
            binding.Endpoint.Address = new EndpointAddress(lr.serverUrl);

            sheader.sessionId = lr.sessionId;

            binding.Endpoint.ListenUri = new Uri(lr.metadataServerUrl);

            foreach (var obj in sfObjects)
            {
                DescribeSObjectResult sobject;
                
                binding.describeSObject(sheader, null, null, null, obj, out sobject);

                metadata.Objects.Add(sobject);
            }

            return new ActionResponse(ActionStatus.Success, metadata);
        }
    }
}
