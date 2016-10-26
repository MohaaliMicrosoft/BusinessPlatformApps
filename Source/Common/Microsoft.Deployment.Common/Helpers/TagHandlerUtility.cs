using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.AppLoad;
using Microsoft.Deployment.Common.Model;
using Microsoft.Deployment.Common.Tags;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Helpers
{
    public static class TagHandlerUtility
    {
        public static void ParseTag(JToken obj, JObject root, App app, Dictionary<string, UIPage> allPages, Dictionary<string, IAction> allActions, IEnumerable<ITagHandler> allTags)
        {
            ParseTag(obj, app, allPages, allActions, allTags, null);
        }

        public static TagReturn ParseTag(JToken obj, App app, Dictionary<string, UIPage> allPages, Dictionary<string, IAction> allActions, IEnumerable<ITagHandler> allTags, TagReturn output)
        {
            if (output == null)
            {
                output = new TagReturn();
            }

            foreach (var tag in allTags)
            {
                if (tag.Tag.Equals(obj.Path.Split('.').Last(), StringComparison.OrdinalIgnoreCase))
                {
                    if (obj.Children().First().Type == JTokenType.Array)
                    {
                        output = tag.ProcessTag(obj.Children().First(), obj, allPages, allActions, app);
                    }
                    else
                    {
                        output = tag.ProcessTag(obj.Value<JToken>(), obj, allPages, allActions, app);
                    }
                }
            }

            if (obj.HasValues && obj.Children().First().Children().Any())
            {
                foreach (var child in obj.Children().First().Children())
                {
                    output = ParseTag(child, app, allPages, allActions, allTags, output);
                }
            }

            return output;
        }
    }
}
