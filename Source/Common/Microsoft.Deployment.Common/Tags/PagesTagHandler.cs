using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.AppLoad;
using Microsoft.Deployment.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Tags
{
    [Export(typeof(ITagHandler))]
    public class PagesTagHandler : ITagHandler
    {
        public string Tag { get; } = "Pages";

        public void ProcessTag(JToken innerJson, JToken entireJson, Dictionary<string, UIPage> allPages, Dictionary<string, IAction> allActions, App app)
        {
            foreach (var child in innerJson.Children())
            {
                string pageName = child["name"].ToString(Formatting.None);
                pageName = pageName.Replace("\"", "");
                pageName = pageName.Replace("/", "\\");

                string pageToSearch = pageName;

                // Find page
                if (!pageName.StartsWith("$"))
                {
                    pageToSearch = $"{app.Name}\\{pageName}";
                }

                if (!allPages.ContainsKey(pageToSearch))
                {
                    throw new Exception($"Page:{pageName} in init.json not found");
                }

                var page = allPages[pageToSearch];

                UIPage pageCopied = page.Clone();
                string displayName = child["displayname"] != null ? child["displayname"].ToString(Formatting.None).Replace("\"", "") : pageName;
                pageCopied.DisplayName = displayName;
                pageCopied.Parameters = child;
                app.Pages.Add(pageCopied);
            }
        }
    }
}
