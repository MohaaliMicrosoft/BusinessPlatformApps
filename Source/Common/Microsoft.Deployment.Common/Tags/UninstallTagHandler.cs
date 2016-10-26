using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.AppLoad;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Deployment.Common.Model;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Tags
{
    [Export(typeof(ITagHandler))]
    class UninstallTagHandler : ITagHandler
    {
        public string Tag { get; } = "Uninstall";

        public TagReturn ProcessTag(JToken innerJson, JToken entireJson, Dictionary<string, UIPage> allPages, Dictionary<string, IAction> allActions, App app)
        {
            IEnumerable<ITagHandler> tags = new List<ITagHandler> { new PagesTagHandler(), new ActionsTagHandler() };
            TagReturn output = new TagReturn();

            foreach (var tag in tags)
            {
                output = TagHandlerUtility.ParseTag(innerJson.Children().FirstOrDefault(), app, allPages, allActions, new List<ITagHandler>() { tag }, null);

                if (output.Output != null && output.Output.GetType() == typeof(List<UIPage>))
                {
                    app.UninstallPages.AddRange(output.Output as IEnumerable<UIPage>);
                }
                if (output.Output != null && output.Output.GetType() == typeof(IEnumerable<DeploymentAction>))
                {
                    app.UninstallActions.AddRange(output.Output as IEnumerable<DeploymentAction>);
                }
            }

            return new TagReturn() { Output = null, Recurse = true };
        }
    }
}
