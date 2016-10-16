using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.Deployment.Common;
using Microsoft.Deployment.Common.Template;

namespace Microsoft.Deployment.Site.Service.Controllers
{
    public class TemplatesController : ApiController
    {
        [HttpGet]
        public IEnumerable<string> GetAllTemplates()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("Service", "Online");

            string operationId = string.Empty;
            string userGenId = string.Empty;
            string userId = string.Empty;
            string sessionId = string.Empty;
            string uniqueId = string.Empty;

            if (this.Request.Headers.Contains("OperationId"))
            {
                operationId = this.Request.Headers.GetValues("OperationId").First();
            }

            if (this.Request.Headers.Contains("UserGeneratedId"))
            {
                userGenId = this.Request.Headers.GetValues("UserGeneratedId").First();
            }

            if (this.Request.Headers.Contains("UserId"))
            {
                userId = this.Request.Headers.GetValues("UserId").First();
            }

            if (this.Request.Headers.Contains("SessionId"))
            {
                sessionId = this.Request.Headers.GetValues("SessionId").First();
            }

            if (this.Request.Headers.Contains("UniqueId"))
            {
                uniqueId = this.Request.Headers.GetValues("UniqueId").First();
            }

            string referer = Request.RequestUri.GetLeftPart(UriPartial.Authority);
            string[] url = Request.Headers.Referrer?.ToString().Split('/');
            if (url?.Length >= 3)
            {
                referer = url[0] + "//" + url[2];
            }

            return new CommonController("API", param, Request.RequestUri.GetLeftPart(UriPartial.Authority), Constants.TemplatePath, referer, WebApiConfig.Templates)
                .GetAllTemplates(userId, userGenId, sessionId, operationId, uniqueId);
        }

        [HttpGet]
        public Template GetTemplate(string id)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("Service", "Online");

            string operationId = string.Empty;
            string userGenId = string.Empty;
            string userId = string.Empty;
            string sessionId = string.Empty;
            string uniqueId = string.Empty;

            if (this.Request.Headers.Contains("OperationId"))
            {
                operationId = this.Request.Headers.GetValues("OperationId").First();
            }

            if (this.Request.Headers.Contains("UserGeneratedId"))
            {
                userGenId = this.Request.Headers.GetValues("UserGeneratedId").First();
            }

            if (this.Request.Headers.Contains("UserId"))
            {
                userId = this.Request.Headers.GetValues("UserId").First();
            }

            if (this.Request.Headers.Contains("SessionId"))
            {
                sessionId = this.Request.Headers.GetValues("SessionId").First();
            }

            if (this.Request.Headers.Contains("UniqueId"))
            {
                uniqueId = this.Request.Headers.GetValues("UniqueId").First();
            }

            string referer = Request.RequestUri.GetLeftPart(UriPartial.Authority);
            string[] url = Request.Headers.Referrer?.ToString().Split('/');
            if (url?.Length >= 3)
            {
                referer = url[0] + "//" + url[2];
            }


            return new CommonController("API", param, Request.RequestUri.GetLeftPart(UriPartial.Authority), Constants.TemplatePath, referer, WebApiConfig.Templates)
                .GetTemplate(userId, userGenId, sessionId, operationId, uniqueId, id);
        }
    }
}