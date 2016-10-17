using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.AppLoad;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Exceptions;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common
{
    public class CommonController
    {
        public CommonController(string source, Dictionary<string, string> loggingParameters,
            string virtualPathRoot, string appRelativePath, string refererUrl, AppFactory appFactory)
        {
            this.Source = source;
            this.LoggingParameters = loggingParameters;
            this.VirtualPathRoot = virtualPathRoot;
            this.AppRelativePath = appRelativePath;
            this.Referer = refererUrl;
            this.AppFactory = appFactory;
        }

        private string Referer { get; set; }
        public AppFactory AppFactory { get; set; }

        public Dictionary<string, string> LoggingParameters { get; private set; }

        private string Source { get; set; }

        private string VirtualPathRoot { get; set; }

        private string AppRelativePath { get; set; }

        public IEnumerable<string> GetAllApps(string userId, string userGenId, string sessionId, string operationId,
            string uniqueLink)
        {
            Logger logger = new Logger(userId, userGenId, sessionId, operationId, uniqueLink, "NA", this.Source,
                "GetAllApps", this.LoggingParameters);

            var start = DateTime.Now;
            var templateNames = this.AppFactory.Apps.Select(p => p.Key);
            var end = DateTime.Now;

            var allTemplates = templateNames as IList<string> ?? templateNames.ToList();
            logger.LogRequest("GetAllTemplates", end - start, allTemplates.Any());
            logger.Flush();
            return allTemplates;
        }

        public App GetApp(string userId, string userGenId, string sessionId, string operationId,
            string uniqueLink, string id)
        {

            Logger logger = new Logger(userId, userGenId, sessionId, operationId, uniqueLink, id, this.Source,
                "GetTemplate", this.LoggingParameters);
            var start = DateTime.Now;
            var template = this.AppFactory.Apps[id];
            var end = DateTime.Now;

            logger.LogRequest("GetTemplate-" + id, end - start, template != null);
            logger.Flush();
            return template;
        }

        public ActionResponse ExecuteAction(string userId, string userGenId, string sessionId, string operationId,
            string uniqueLink, string templateName, string id, JObject body)
        {
            Logger logger = new Logger(userId, userGenId, sessionId, operationId, uniqueLink, templateName, this.Source,
                id, this.LoggingParameters);
            logger.LogEvent("Start-" + id, null);
            var start = DateTime.Now;
            var action = this.AppFactory.Actions[id];
            if (action != null)
            {
                int loopCount = 0;

                ActionResponse responseToReturn = RunAction(templateName, body, logger, action, loopCount);
                logger.LogEvent("End-" + id, null, body, responseToReturn);
                logger.LogRequest(action.OperationUniqueName, DateTime.Now - start,
                    responseToReturn.Status.IsSucessfullStatus(), body, responseToReturn);
                logger.Flush();
                return responseToReturn;
            }

            logger.LogEvent("End-" + id, null, body, null);
            logger.LogRequest(id, DateTime.Now - start, false, body, null);
            var ex = new ActionNotFoundException();
            logger.LogException(ex, null, body, null);
            logger.Flush();
            throw ex;
        }

        private ActionResponse RunAction(string templateName, JObject body, Logger logger,
            IAction action, int loopCount)
        {
            ActionResponse responseToReturn = null;
            do
            {
                ActionRequest request = new ActionRequest(this.LoggingParameters, body, templateName,
                       this.VirtualPathRoot, this.AppRelativePath, this.Referer, this.AppFactory.Actions);
                request.Logger = logger;

                try
                {
                    responseToReturn = this.RunActionWithInterceptor(action, request);
                }
                catch (Exception exceptionFromAction)
                {
                    responseToReturn = RunExceptionHandler(request,exceptionFromAction);
                }

                loopCount += 1;
            } while (loopCount <= 1 && responseToReturn.Status == ActionStatus.Retry);

            if (responseToReturn.Status == ActionStatus.Retry)
            {
                responseToReturn.Status = ActionStatus.Failure;
            }

            return responseToReturn;
        }

        private ActionResponse RunExceptionHandler( ActionRequest request, Exception exceptionFromAction)
        {
            ActionResponse responseToReturn = null;
            if (exceptionFromAction is AggregateException)
            {
                AggregateException exc = exceptionFromAction as AggregateException;
                exceptionFromAction = exc.GetBaseException();
            }

            var exceptionHandler = this.AppFactory.ActionExceptionsHandlers
                .FirstOrDefault(p => p.ExceptionExpected == exceptionFromAction.GetType());

            bool showGenericException = true;

            if (exceptionHandler != null)
            {
                try
                {
                    request.Logger.LogEvent("StartExceptionHandler-" + exceptionFromAction.GetType().Name, null);
                    responseToReturn = exceptionHandler.HandleException(request, exceptionFromAction);
                    showGenericException = false;
                }
                catch
                {
                }
                finally
                {
                    request.Logger.LogEvent("EndExceptionHandler-" + exceptionFromAction.GetType().Name, null, null,
                        responseToReturn);
                }
            }

            if (showGenericException || responseToReturn.Status == ActionStatus.UnhandledException)
            {
                responseToReturn = new ActionResponse(ActionStatus.Failure, null, exceptionFromAction,
                    DefaultErrorCodes.DefaultErrorCode);
                request.Logger.LogException(exceptionFromAction, null);
            }

            return responseToReturn;
        }

        private ActionResponse RunActionWithInterceptor(IAction action, ActionRequest request)
        {
            ActionResponse responseToReturn;

            var interceptors = this.AppFactory.RequestInterceptors.Select(p => 
            new Tuple<InterceptorStatus, IActionRequestInterceptor>(p.CanIntercept(action, request), p))
            .ToList();

            // This code handles all interceptors which dont affect the action execution
            // Token refreshes, db creation tasks and generally actions which need prework before they
            // can be executed
            var requestInterceptors = interceptors.Where(p => p.Item1 == InterceptorStatus.Intercept);
            foreach (var requestInterceptor in requestInterceptors)
            {
                request.Logger.LogEvent("StartIntercept-" + requestInterceptor.GetType(), null);
                var response = requestInterceptor.Item2.Intercept(action, request);

                Dictionary<string,string> interceptorResult = new Dictionary<string, string>();
                interceptorResult.Add("InterceptorStatus", response.Status.ToString());

                request.Logger.LogEvent("InterceptorResult", interceptorResult, request.Message, response);
                request.Logger.LogEvent("EndIntercept-" + requestInterceptor.GetType(), null);
            }


            // Check to make sure there is only one interceptor which can handle action otherwise use default
            // This could be either (delegate/elevated/non elevated handler)
            var interceptor = interceptors.SingleOrDefault(p => p.Item1 == InterceptorStatus.IntercepAndHandleAction);
            if (interceptor != null)
            { 
                try
                {
                    // No need to log as it will be picked up by the caller
                    request.Logger.LogEvent("StartIntercepAndHandleAction-" + interceptor.GetType(), null);
                    responseToReturn = interceptor.Item2.Intercept(action, request);
                }
                finally
                {
                    request.Logger.LogEvent("EndIntercepAndHandleAction-" + interceptor.GetType(), null);
                }
            }
            else
            {
                // Execute default if none found
                responseToReturn = action.ExecuteAction(request);
            }

            return responseToReturn;
        }
    }
}
