using System.Linq;
using System.Web.Http;
using Microsoft.Deployment.Common.Template;

namespace Microsoft.Deployment.Site.Service
{
    public static class WebApiConfig
    {
        public static TemplateParser Templates { get; private set; }

        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.MessageHandlers.Add(new OptionsHttpMessageHandler());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            Templates = new TemplateParser();
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);
            //config.Services.Add(typeof(IExceptionLogger), new AiExceptionLogger());
        }
    }
}