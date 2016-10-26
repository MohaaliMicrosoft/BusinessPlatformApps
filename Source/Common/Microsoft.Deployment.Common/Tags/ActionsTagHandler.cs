using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.AppLoad;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Deployment.Common.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Tags
{
    [Export(typeof(ITagHandler))]
    public class ActionsTagHandler : ITagHandler
    {
        public string Tag { get; } = "Actions";

        public TagReturn ProcessTag(JToken innerJson, JToken entireJson, Dictionary<string, UIPage> allPages, Dictionary<string, IAction> allActions, App app)
        {
            List<IAction> actionsToReturn = new List<IAction>();

            foreach (var child in innerJson.Children())
            {
                string actionName = child["name"].ToString(Formatting.None).Replace("\"", "");

                var action = allActions[actionName];


                string displayName = child["displayname"] != null ? child["displayname"].ToString(Formatting.None).Replace("\"", "") : actionName;
                DeploymentAction deploymentAction = new DeploymentAction(displayName, action, child);
                actionsToReturn.Add(deploymentAction as IAction);
            }

            return new TagReturn() { Recurse = false, Output = actionsToReturn };
        }
    }
}