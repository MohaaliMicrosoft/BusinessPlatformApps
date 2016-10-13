using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Actions
{
    public class ActionRequest
    {
        public string TemplateRootPath
        {
            get
            {
                string folderPath = System.AppDomain.CurrentDomain.BaseDirectory;
                string templatePath = folderPath + Constants.TemplatePath;
                if (!Directory.Exists(templatePath))
                {
                    templatePath = folderPath + Constants.TemplatePathMsi;
                    if (!Directory.Exists(templatePath))
                    {
                        throw new Exception("Template Root Path invalid");
                    }
                }
                return templatePath;
            }
        }

        public string WebsiteRootUrl { get; set; }

        public string SiteRootPath { get; private set; }

        public string TemplatePath
        {
            get
            {
                return Path.Combine(this.TemplateRootPath, this.TemplateName);
            }
        }

        public Dictionary<string, string> Parameters { get; set; }

        public JObject Message { get; set; }

        public string TemplateName { get; set; }
        public string SiteTemplatePath { get; private set; }
        public Logger Logger { get; set; }

        public IEnumerable<IAction> AllActions { get; set; }

        public ActionRequest(Dictionary<string,string> parameters, JObject message, string template, 
            string siteRoot, string templateRelativePath, string referer, IEnumerable<IAction> allActions)
        {
            this.TemplateName = template;
            this.Parameters = parameters;
            this.Message = message;
            this.SiteRootPath = siteRoot;
            this.SiteTemplatePath = siteRoot + templateRelativePath + "/" + template;
            this.WebsiteRootUrl = referer;
            this.AllActions = allActions;
        }
    }
}