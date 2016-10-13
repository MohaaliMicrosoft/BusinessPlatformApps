using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Deployment.Common.Template;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Tags
{
    [Export(typeof(ITagHandler))]
    public class MsiTagHandler : ITagHandler
    {
        public string Tag { get; } = "MSI";

        public void ProcessTag(JToken innerJson, JToken entireJson, IEnumerable<UIPage> allPages, IEnumerable<IAction> allActions, Template.Template template)
        {
            var val = innerJson.Children()["Guid"].First();

            template.MsiGuid = Guid.Parse(val.ToString());
        }
    }
}
