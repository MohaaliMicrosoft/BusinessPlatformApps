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
    class InstallTagHandler : ITagHandler
    {
        public string Tag { get; } = "Install";

        public TagReturn ProcessTag(JToken innerJson, JToken entireJson, Dictionary<string, UIPage> allPages, Dictionary<string, IAction> allActions, App app)
        {
            IEnumerable<ITagHandler> tags = new List<ITagHandler> { new PagesTagHandler(), new ActionsTagHandler() };

            foreach (var child in innerJson.Children().FirstOrDefault().Children())
            {
                var output = TagHandlerUtility.ParseTag(child, app, allPages, allActions, tags, null);

                if (output.Output != null && output.Output.GetType() == typeof(List<UIPage>))
                {
                    app.InstallPages.AddRange((output.Output as List<UIPage>));
                }
                if (output.Output != null && output.Output.GetType() == typeof(List<IAction>))
                {
                    app.InstallActions.AddRange(output.Output as List<DeploymentAction>);
                }
            }
            return new TagReturn() { Output = null, Recurse = true };
        }
    }
}
