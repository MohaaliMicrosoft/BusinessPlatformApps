using System.Collections.Generic;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.AppLoad;
using Microsoft.Deployment.Common.Model;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Tags
{
    public interface ITagHandler
    {
        string Tag { get; }

        TagReturn ProcessTag(JToken innerJson, JToken entireJson, Dictionary<string,UIPage> allPages, Dictionary<string, IAction> allActions, App app);
    }
}