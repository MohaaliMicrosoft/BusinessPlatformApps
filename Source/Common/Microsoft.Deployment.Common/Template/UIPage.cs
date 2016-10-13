using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Template
{
    public class UIPage
    {
        public string PageName { get; set; }
        public string RoutePageName { get; set; }
        public string Path { get; set; }
        public string TemplateName { get; set; }
        public string DisplayName { get; set; }

        public JToken AdditionalParameters { get; set; }

        public UIPage Clone()
        {
            return new UIPage()
            {
                PageName = this.PageName,
                RoutePageName = this.RoutePageName,
                Path = this.Path,
                TemplateName = this.TemplateName,
                DisplayName = this.DisplayName,
                AdditionalParameters = this.AdditionalParameters
            };
        }
    }
}