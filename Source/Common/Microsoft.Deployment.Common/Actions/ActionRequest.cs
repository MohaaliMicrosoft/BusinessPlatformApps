using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Actions
{
    public class ActionRequest
    {
        public string AppsRootPath { get; set; }
        public string WebsiteRootUrl { get; set; }
        public string ServiceRootPath { get; private set; }
        public string AppPath { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public JObject Message { get; set; }
        public string AppName { get; set; }
        public string SiteAppPath { get; private set; }
        public Logger Logger { get; set; }
        public Dictionary<string, IAction> Actions { get; set; }

        public ActionRequest(Dictionary<string,string> parameters, JObject message, string appName, 
            string serviceRoot, string appRelativePath, string referer, Dictionary<string,IAction> actions)
        {
            this.AppName = appName;
            this.Parameters = parameters;
            this.Message = message;
            this.ServiceRootPath = serviceRoot;
            this.SiteAppPath = Path.Combine(serviceRoot, appRelativePath,appName);
            this.WebsiteRootUrl = referer;
            this.Actions = actions;
        }
    }
}