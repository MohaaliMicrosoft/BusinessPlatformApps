using System.ComponentModel.Composition;

using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Helpers;
using Microsoft.Win32;

namespace Microsoft.Bpst.Actions.OnPremiseActions.WinNT
{
    // Should not run impersonated
    [Export(typeof(IAction))]
    public class ValidateSecurityOptions : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Lsa"))
            {
                if (key == null)
                    return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());

                object v = key.GetValue("disabledomaincreds");

                if (v != null && (int)v == 1)
                    return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "DisabledDomainCredsEnabled");
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }
    }
}