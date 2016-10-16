using System.ComponentModel.Composition;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Common.Actions
{
    [Export(typeof(IActionRequestInterceptor))]
    public class DelegateInterceptor : IActionRequestInterceptor
    {
        public InterceptorStatus CanIntercept(IAction actionToExecute, ActionRequest request)
        {
            bool? impersonationFound = request.Message
                .SelectToken("ImpersonateAction")?
                .ToString()
                .EqualsIgnoreCase("true");

            if (impersonationFound.HasValue && impersonationFound.Value)
            {
                return InterceptorStatus.IntercepAndHandleAction;
            }

            return InterceptorStatus.Skipped;
        }

        public ActionResponse Intercept(IAction actionToExecute, ActionRequest request)
        {
            return ImpersonateUtility.Execute(actionToExecute.ExecuteAction, request);
        }
    }
}
