using System;
using System.ComponentModel.Composition;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.OnPremiseActions.WinNT
{
    [Export(typeof(IAction))]
    public class AddLogonAsBatchPermission : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string domain = request.Message["ImpersonationDomain"] == null || string.IsNullOrEmpty(request.Message["ImpersonationDomain"][0].ToString())
                ? Environment.GetEnvironmentVariable("USERDOMAIN")
                : request.Message["ImpersonationDomain"][0].ToString();
            string user = request.Message["ImpersonationUsername"] == null || string.IsNullOrEmpty(request.Message["ImpersonationUsername"][0].ToString())
                ? Environment.GetEnvironmentVariable("USERNAME")
                : request.Message["ImpersonationUsername"][0].ToString();

            string domainAccount = $"{NTHelper.CleanDomain(domain)}\\{NTHelper.CleanUsername(user)}";

            // This will throw an error if the permission cannot be granted
            NTPermissionUtility.SetRight(Environment.MachineName, domainAccount, NTPermissionUtility.LOGON_AS_BATCH_PERM, false);

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }
    }
}