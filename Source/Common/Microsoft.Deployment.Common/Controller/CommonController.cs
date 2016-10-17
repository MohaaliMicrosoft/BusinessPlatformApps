using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.AppLoad;
using Microsoft.Deployment.Common.Controller;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Exceptions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Microsoft.Deployment.Common
{
    public class CommonController
    {
        public CommonController(CommonControllerModel commonControllerModel)
        {
           
        }

        public CommonControllerModel CommonControllerModel { get; set; }


        public IEnumerable<string> GetAllApps(UserInfo info)
        {
            Logger logger = new Logger(info, this.CommonControllerModel);
            info.ActionName = "GetAllApps";

            var start = DateTime.Now;
            var templateNames = this.CommonControllerModel.AppFactory.Apps.Select(p => p.Key);
            var end = DateTime.Now;

            var allTemplates = templateNames as IList<string> ?? templateNames.ToList();
            logger.LogRequest("GetAllApps", end - start, allTemplates.Any());
            logger.Flush();
            return allTemplates;
        }

        public App GetApp(UserInfo info)
        {

            Logger logger = new Logger(info, this.CommonControllerModel);
            info.ActionName = "GetApp";

            var start = DateTime.Now;
            var app = this.CommonControllerModel.AppFactory.Apps[info.AppName];
            var end = DateTime.Now;

            logger.LogRequest("GetApp-" + info.AppName, end - start, app != null);
            logger.Flush();
            return app;
        }

        public ActionResponse ExecuteAction(UserInfo info, JObject body, string actionName)
        {
            info.ActionName = actionName;
            Logger logger = new Logger(info, this.CommonControllerModel);
            logger.LogEvent("Start-" + actionName, null);
            var start = DateTime.Now;
            var action = this.CommonControllerModel.AppFactory.Actions[actionName];

            ActionRequest request = JsonConvert.DeserializeObject<ActionRequest>(body.ToString());
            request.ControllerModel = this.CommonControllerModel;
            request.Info = info;
            request.Logger = logger;

            if (action != null)
            {
                int loopCount = 0;

                ActionResponse responseToReturn = RunAction(request, logger, action, loopCount);
                logger.LogEvent("End-" + actionName, null, request, responseToReturn);
                logger.LogRequest(action.OperationUniqueName, DateTime.Now - start,
                    responseToReturn.Status.IsSucessfullStatus(), request, responseToReturn);
                logger.Flush();
                return responseToReturn;
            }

            logger.LogEvent("End-" + actionName, null, request, null);
            logger.LogRequest(actionName, DateTime.Now - start, false, request, null);
            var ex = new ActionNotFoundException();
            logger.LogException(ex, null, request, null);
            logger.Flush();
            throw ex;
        }

        private ActionResponse RunAction(ActionRequest request, Logger logger,
            IAction action, int loopCount)
        {
            ActionResponse responseToReturn = null;
            do
            {
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

            var exceptionHandler = this.CommonControllerModel.AppFactory.ActionExceptionsHandlers
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

            var interceptors = this.CommonControllerModel.AppFactory.RequestInterceptors.Select(p => 
            new Tuple<InterceptorStatus, IActionRequestInterceptor>(p.CanIntercept(action, request), p))
            .ToList();

            // This code handles all interceptors which dont affect the action execution
            // Token refreshes, db creation tasks and generally actions which need prework before they
            // can be executed
            var requestInterceptors = interceptors.Where(p => p.Item1 == InterceptorStatus.Intercept);
            foreach (var requestInterceptor in requestInterceptors)
            {
                var response = requestInterceptor.Item2.Intercept(action, request);
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
