using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Template;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Tags
{
    [Export(typeof(ITagHandler))]
    public class TestTagHandler : ITagHandler
    {
        public string Tag { get; } = "Test";

        public void ProcessTag( JToken innerJson, JToken entireJson, IEnumerable<UIPage> allPages, IEnumerable<IAction> allActions, Template.Template template)
        {
        }
    }
}