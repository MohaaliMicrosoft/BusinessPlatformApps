using System.Linq;

using Microsoft.Deployment.Common.Actions;

namespace Microsoft.Deployment.Common.Helpers
{
    public class RequestUtility
    {
        public static ActionResponse CallAction(ActionRequest request, string actionName)
        {
            var action = request.AllActions.SingleOrDefault(p => p.OperationUniqueName.EqualsIgnoreCase(actionName));
            return action.ExecuteAction(request);
        }
    }
}