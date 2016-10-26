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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Tags
{
    [Export(typeof(ITagHandler))]
    public class MsiTagHandler : ITagHandler
    {
        public string Tag { get; } = "MSI";

        public TagReturn ProcessTag(JToken innerJson, JToken entireJson, Dictionary<string, UIPage> allPages, Dictionary<string, IAction> allActions, App app)
        {
            var val = innerJson.Children()["Guid"].First();

            app.MsiGuid = Guid.Parse(val.ToString());

            return new TagReturn() { Output = null, Recurse = false};
        }
    }
}
