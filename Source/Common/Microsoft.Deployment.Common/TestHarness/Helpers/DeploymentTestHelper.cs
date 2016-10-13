using System.Linq;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.TestHarness.Helpers
{
    public class DeploymentTestHelper
    {
        // This method will implement the way the website does its deployment
        public static void DeployTemplate(string templateToDeploy, JObject payload, int stepToStart = 0)
        {
            var template = TestService.Instance.TemplateUtility.Templates
                .SingleOrDefault(p => p.TemplateName.EqualsIgnoreCase(templateToDeploy));

            DataStoreMock datastore = new DataStoreMock();
            datastore.AddObjectToDataStore("deployment-1", payload);

            ActionStatus lastStatus = ActionStatus.Success;

            for (int index = stepToStart; index < template.Actions.Count; index++)
            {
                var action = template.Actions[index];
                var payloadCopy = datastore.GetDataStore();

                if (lastStatus != ActionStatus.BatchWithState )
                {
                    // Add on additional parameters - these tags overwrite original tags
                    action.AdditionalParameters.ToList().ForEach(p =>
                    {
                        if (payloadCopy.SelectToken(p.Path.Split('.').Last()) != null)
                        {
                            payloadCopy.Remove(p.Path.Split('.').Last());
                        }

                        payloadCopy.Add(p);
                    });
                }

                var response = TestService.Instance.ExecuteAction(templateToDeploy, action.OperationName, payloadCopy);

                if (response.Status == ActionStatus.BatchWithState || response.Status == ActionStatus.BatchNoState)
                {
                    index = index - 1;
                }
                
                lastStatus = response.Status;
                datastore.AddObjectToDataStore("Deployment" + index, response.Response);
            }
        }
    }
}
