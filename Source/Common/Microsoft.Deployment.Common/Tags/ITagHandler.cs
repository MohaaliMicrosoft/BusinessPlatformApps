using System.Collections.Generic;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.AppLoad;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Tags
{
    public interface ITagHandler
    {
        string Tag { get; }

        void ProcessTag(JToken innerJson, JToken entireJson, Dictionary<string,UIPage> allPages, Dictionary<string, IAction> allActions, App app);
    }
}