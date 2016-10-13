using System.Collections.Generic;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Template;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Tags
{
    public interface ITagHandler
    {
        string Tag { get; }

        void ProcessTag(JToken innerJson, JToken entireJson, IEnumerable<UIPage> allPages, IEnumerable<IAction> allActions, Template.Template template);
    }
}