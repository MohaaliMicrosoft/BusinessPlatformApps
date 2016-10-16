using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Deployment.Common.Template;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Tags
{
    [Export(typeof(ITagHandler))]
    public class PagesTagHandler : ITagHandler
    {
        public string Tag { get; } = "Pages";

        public void ProcessTag(JToken innerJson, JToken entireJson, IEnumerable<UIPage> allPages, IEnumerable<IAction> allActions, Template.Template template)
        {
            foreach (var child in innerJson.Children())
            {
                string pageName = child["name"].ToString(Formatting.None).Replace("\"", "");

                var page = allPages.FirstOrDefault(
                        p => ((p.TemplateName.EqualsIgnoreCase(template.TemplateName)
                              || p.TemplateName.EqualsIgnoreCase("CommonUI")) 
                              && p.PageName.EqualsIgnoreCase(pageName)));

                if (page == null)
                {
                    throw new Exception("Unable to find page");
                }

                UIPage pageCopied = page.Clone();

                string displayName = child["displayname"] != null ? child["displayname"].ToString(Formatting.None).Replace("\"", "") : pageName;
                pageCopied.DisplayName = displayName;
                pageCopied.AdditionalParameters = child;

                template.Pages.Add(pageCopied);
            }
         
        }
    }
}
