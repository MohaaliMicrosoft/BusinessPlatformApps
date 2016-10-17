using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Common.Actions
{
    [Export(typeof(IActionRequestInterceptor))]
    public class DelegateInterceptor : IActionRequestInterceptor
    {
        public InterceptorStatus CanIntercept(IAction actionToExecute, ActionRequest request)
        {
            bool impersonationFound = request.DataStore.PublicDataStore.ContainsKey("ImpersonateAction") &&
            request.DataStore.GetValueFromDataStore(DataStoreType.Any, "ImpersonateAction").First().ToString() == "true";

            if (impersonationFound)
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
