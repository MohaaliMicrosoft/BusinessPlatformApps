using System;
using System.Collections.Generic;
using Microsoft.Deployment.Common.Actions;

namespace Microsoft.Deployment.Common.AppLoad
{
    public class App
    {
        public App()
        {
            this.Pages = new List<UIPage>();
            this.Actions = new List<DeploymentAction>();
        }
        public string Name { get; set; }

        public List<UIPage> Pages { get; set; }
        public List<DeploymentAction> Actions { get; set; }

        public Guid MsiGuid { get; set; } = Guid.Empty;
    }
}