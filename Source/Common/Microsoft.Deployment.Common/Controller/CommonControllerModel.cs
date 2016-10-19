﻿using System.Collections.Generic;
using Microsoft.Deployment.Common.AppLoad;

namespace Microsoft.Deployment.Common.Controller
{
    public class CommonControllerModel
    {
        public AppFactory AppFactory { get; set; }
        public Dictionary<string, string> LoggingParameters { get; set; }
        public string Source { get; set; }
        public string VirtualPathRoot { get; set; }
        public string AppRootPath { get; set; }
        public string SiteCommonPath { get; set; }
        public string Referer { get; set; }
    }
}
