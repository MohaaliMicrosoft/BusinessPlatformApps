using System.ComponentModel.Composition;
using System.Security.Principal;

using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.OnPremiseActions.WinNT
{
    // Should not run impersonated
    [Export(typeof(IAction))]
    public class ValidateAdminPrivileges : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            WindowsPrincipal current = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            return current.IsInRole(WindowsBuiltInRole.Administrator)
                ? new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject())
                : new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "NotAdmin");
        }
    }
}