using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Deployment.Common.Template;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Tags
{
    [Export(typeof(ITagHandler))]
    public class ActionsTagHandler : ITagHandler
    {
        public string Tag { get; } = "Actions";

        public void ProcessTag(JToken innerJson, JToken entireJson, IEnumerable<UIPage> allPages, IEnumerable<IAction> allActions, Template.Template template)
        {
            foreach (var child in innerJson.Children())
            {
                string actionName = child["name"].ToString(Formatting.None).Replace("\"", "");

                var action = allActions.FirstOrDefault(p => p.OperationUniqueName.EqualsIgnoreCase(actionName));
                              
                if (action == null)
                {
                    //throw new Exception("Unable to find action");
                }
                else
                {
                    string displayName = child["displayname"] != null ? child["displayname"].ToString(Formatting.None).Replace("\"", "") : actionName;
                    DeploymentAction deploymentAction = new DeploymentAction(displayName, action, child);
                    template.Actions.Add(deploymentAction);
                }
            }
        }
    }
}