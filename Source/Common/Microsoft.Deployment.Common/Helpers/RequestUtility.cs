using System.Linq;
using Microsoft.Deployment.Common.ActionModel;

namespace Microsoft.Deployment.Common.Helpers
{
    public class RequestUtility
    {
        public static ActionResponse CallAction(ActionRequest request, string actionName)
        {
            var action = request.ControllerModel.AppFactory.Actions[actionName];
            return action.ExecuteAction(request);
        }
    }
}