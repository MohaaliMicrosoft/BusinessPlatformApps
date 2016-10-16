namespace Microsoft.Deployment.Common
{
    using System;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights;
    using Microsoft.Deployment.Common.Actions;
    using Microsoft.Deployment.Common.Helpers;
    using Newtonsoft.Json.Linq;
    using System.Text.RegularExpressions;

    public class Logger
    {
        private TelemetryClient telemetryClient;

        private string actionName;

        private Dictionary<string, string> globalParams;

        private string source;

        public Logger(string userId, string userGenId, string sessionId, string operationId, string uniqueLink, string template, string source, string actionName, Dictionary<string, string> globalParams)
        {
            this.source = source;
            this.actionName = actionName;
            this.telemetryClient = new TelemetryClient { InstrumentationKey = Constants.AppInsightsKey };

            this.globalParams = globalParams;

            this.telemetryClient.Context.Operation.Id = operationId;
            this.telemetryClient.Context.User.Id = userId;
            this.telemetryClient.Context.Session.Id = sessionId;
            this.telemetryClient.Context.Operation.Name = this.actionName;

            if (this.globalParams == null)
            {
                this.globalParams = new Dictionary<string, string>();
            }

            this.globalParams.Add("TemplateName", template);
            this.globalParams.Add("UserGenId", userGenId);
            this.globalParams.Add("EntryPointAction", this.actionName);
            this.globalParams.Add("OriginatingSource", this.source);
            this.globalParams.Add("LinkedOperationId", operationId);
            this.globalParams.Add("UniqueLink", uniqueLink);
            this.globalParams.Add("Build", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            this.AddToDictionary(this.globalParams, this.telemetryClient.Context.Properties);
        }

        public void LogEvent(string eventName, Dictionary<string, string> properties)
        {
            this.telemetryClient.TrackEvent(eventName, properties);
            this.Flush();
        }

        public void LogCustomProperty(string propKey, string propValue)
        {
            Dictionary<string, string> customProperty = new Dictionary<string, string>();
            customProperty.Add("Custom Property Key", propKey);
            customProperty.Add("Custom Property Value", propValue);
            this.LogEvent("Custom-Property", customProperty);
        }

        public void LogMetric(string metricName, double value, Dictionary<string, string> properties)
        {
            this.telemetryClient.TrackMetric(metricName, value, properties);
            this.Flush();
        }

        public void LogPageView(string page)
        {
            this.telemetryClient.TrackPageView(page);
            this.Flush();
        }

        public void LogRequest(string request, TimeSpan duration, bool sucess)
        {
            string respone = sucess ? "200" : "504";
            this.telemetryClient.TrackRequest(request, DateTimeOffset.Now, duration, respone, sucess);
            this.Flush();
        }

        public void LogDependancyCall(string dependancy, string callName, TimeSpan duration, bool sucess)
        {
            this.telemetryClient.TrackDependency(dependancy, callName, DateTime.Now, duration, sucess);
        }

        public void LogException(Exception exception, Dictionary<string, string> properties = null)
        {
            this.telemetryClient.TrackException(exception, properties);
            this.Flush();
        }

        public void LogTrace(string objectString, object obj, string traceId)
        {
            Dictionary<string, object> container = new Dictionary<string, object>();
            container.Add("TraceId", traceId);
            container.Add(objectString, RemovePrivateInformation(obj));

            foreach (var globParam in this.globalParams)
            {
                container.Add(globParam.Key, globParam.Value);
            }

            this.AddTraceId(traceId);

            this.telemetryClient.TrackTrace(JsonUtility.GetJsonStringFromObject(container));

            this.Flush();
            this.ClearTraceId();
        }

        public void Flush()
        {
            this.telemetryClient.Flush();
        }

        public void AddTraceId(string traceId)
        {
            if (!this.telemetryClient.Context.Properties.Keys.Contains("TraceId"))
            {
                this.telemetryClient.Context.Properties.Add("TraceId", traceId);
            }

            this.telemetryClient.Context.Properties["TraceId"] = traceId;
        }

        public void ClearTraceId()
        {
            if (this.telemetryClient.Context.Properties.Keys.Contains("TraceId"))
            {
                this.telemetryClient.Context.Properties.Remove("TraceId");
            }
        }

        internal void LogEvent(string eventName, Dictionary<string, string> properties, JObject requestBody, ActionResponse responseToReturn)
        {
            // Add the reqyest/response onto body
            if (properties == null)
            {
                properties = new Dictionary<string, string>();
            }

            string traceId = Guid.NewGuid().ToString();
            this.AddTraceId(traceId);
            this.LogTrace("RequestBody", requestBody, traceId);
            this.LogTrace("ResponseBody", responseToReturn, traceId);
            this.telemetryClient.TrackEvent(eventName, properties);
            this.Flush();
            this.ClearTraceId();
        }

        internal void LogException(Exception exception, Dictionary<string, string> properties, JObject requestBody, ActionResponse responseToReturn)
        {
            // Add the reqyest/response onto body
            if (properties == null)
            {
                properties = new Dictionary<string, string>();
            }

            string traceId = Guid.NewGuid().ToString();
            this.AddTraceId(traceId);
            this.LogTrace("RequestBody", requestBody, traceId);
            this.LogTrace("ResponseBody", responseToReturn, traceId);
            this.telemetryClient.TrackException(exception, properties);
            this.Flush();
            this.ClearTraceId();
        }

        internal void LogRequest(string request, TimeSpan duration, bool sucess, JObject requestBody, ActionResponse responseToReturn)
        {
            if (responseToReturn == null)
            {
                responseToReturn = new ActionResponse(ActionStatus.Failure);
            }

            string respone = responseToReturn.Status.ResponseCode();

            // Add the request/response onto body
            string traceId = Guid.NewGuid().ToString();
            this.AddTraceId(traceId);
            this.LogTrace("RequestBody", requestBody, traceId);
            this.LogTrace("ResponseBody", responseToReturn, traceId);

            this.telemetryClient.TrackRequest(request, DateTimeOffset.Now, duration, respone, sucess);
            this.Flush();
            this.ClearTraceId();
        }

        private void AddToDictionary(IDictionary<string, string> properties, IDictionary<string, string> targetDictionary)
        {
            foreach (var prop in properties)
            {
                targetDictionary.Add(prop.Key, prop.Value);
            }
        }

        private object RemovePrivateInformation(object o)
        {
            if (o is JObject)
            {
                JObject result = new JObject((JObject)o);
                result.Remove("ImpersonationPassword");
                result.Remove("SqlCredentials");
                result.Remove("Token");

                return result;
            }

            return o;
        }
    }
}