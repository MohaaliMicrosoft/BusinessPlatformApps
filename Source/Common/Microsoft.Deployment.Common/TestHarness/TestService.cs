using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Deployment.Common;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Deployment.Common.Template;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.TestHarness
{
    public class TestService
    {
        public static TestService Instance { get; set; }
        public TemplateParser TemplateUtility { get; set; } = new TemplateParser();

        public const string TestRedirectUrl = "https://bpst-slot1.azurewebsites.net";

        public bool UseRealService { get; } = false;

        public string ActionUrl { get; } = "https://bpstservicebackup2.azurewebsites.net/api/action/";

        public TestService()
        {
            Instance = this;
        }

        public ActionResponse ExecuteAction(string method, dynamic payload, Dictionary<string, string> param = null, bool performLocal = false)
        {
            if (UseRealService && !performLocal)
            {
                HttpClientUtility client = new HttpClientUtility();
                Dictionary<string, string> headers = new Dictionary<string, string>()
                {
                    {"OperationId","Test"},
                    {"UserGeneratedId","Test"},
                    {"TemplateName","Microsoft-TwitterTemplate"},
                    {"UserId","Test"},
                    {"SessionId","Test"},
                    {"UniqueId","Test" }
                };

                HttpResponseMessage result = client.ExecuteGenericAsync(HttpMethod.Post, this.ActionUrl + method,
                    JsonUtility.GetJsonStringFromObject(payload), "application/json", headers).Result;
                return JsonConvert.DeserializeObject<ActionResponse>(result.Content.ReadAsStringAsync().Result);
            }
            return this.ExecuteAction(method, JsonUtility.GetJObjectFromObject(payload), param, performLocal);
        }

        public ActionResponse ExecuteAction(string method, JObject payload, Dictionary<string, string> param = null, bool performLocal = false)
        {
            if (UseRealService && !performLocal)
            {

                HttpClientUtility client = new HttpClientUtility();
                Dictionary<string, string> headers = new Dictionary<string, string>()
                {
                    {"OperationId","Test"},
                    {"UserGeneratedId","Test"},
                    {"TemplateName","Microsoft-TwitterTemplate"},
                    {"UserId","Test"},
                    {"SessionId","Test"},
                    {"UniqueId","Test" }
                };

                HttpResponseMessage result = client.ExecuteGenericAsync(HttpMethod.Post, this.ActionUrl + method,
                    JsonUtility.GetJsonStringFromObject(payload), "application/json", headers).Result;
                return JsonConvert.DeserializeObject<ActionResponse>(result.Content.ReadAsStringAsync().Result);
            }
            return this.ExecuteAction("Microsoft-TwitterTemplate", method, payload, param, performLocal);
        }

        public ActionResponse ExecuteAction(string template, string method, JObject payload, Dictionary<string, string> param = null, bool performLocal = false)
        {
            if (param == null)
            {
                param = new Dictionary<string, string>();
            }

            Dictionary<string, string> loggingParam = new Dictionary<string, string>();
            param.Add("Service", "Test");
            var service = new CommonController("Test", loggingParam, TestRedirectUrl, Constants.TemplatePath,
                TestRedirectUrl, this.TemplateUtility);

            if (UseRealService && !performLocal)
            {

                HttpClientUtility client = new HttpClientUtility();
                Dictionary<string, string> headers = new Dictionary<string, string>()
                {
                    {"OperationId","Test"},
                    {"UserGeneratedId","Test"},
                    {"TemplateName",template},
                    {"UserId","Test"},
                    {"SessionId","Test"},
                    {"UniqueId","Test" }
                };

                HttpResponseMessage result = client.ExecuteGenericAsync(HttpMethod.Post, this.ActionUrl + method,
                    JsonUtility.GetJsonStringFromObject(payload), "application/json", headers).Result;
                return JsonConvert.DeserializeObject<ActionResponse>(result.Content.ReadAsStringAsync().Result);
            }

            return service.ExecuteAction("123", "123", "123", "123", "123", template,
             method, JsonUtility.GetJObjectFromObject(payload));
        }
    }
}
