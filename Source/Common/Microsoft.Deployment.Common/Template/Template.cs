using System;
using System.Collections.Generic;
using Microsoft.Deployment.Common.Actions;

namespace Microsoft.Deployment.Common.Template
{
    public class Template
    {
        public Template()
        {
            this.Pages = new List<UIPage>();
            this.Actions = new List<DeploymentAction>();
        }

        public string TemplateName { get; set; }
        public List<UIPage> Pages { get; set; }
        public List<DeploymentAction> Actions { get; set; }
        public List<string> DependantActionsFolder { get; set; }
        public Guid MsiGuid { get; set; } = Guid.Empty;
    }
}