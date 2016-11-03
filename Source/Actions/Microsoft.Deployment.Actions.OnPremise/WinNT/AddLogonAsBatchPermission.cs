namespace Microsoft.Deployment.Actions.OnPremise.WinNT
{
    using Microsoft.Deployment.Common.ActionModel;
    using Microsoft.Deployment.Common.Actions;
    using Microsoft.Deployment.Common.Helpers;
    using System;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;

    [Export(typeof(IAction))]
    public class AddLogonAsBatchPermission : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string domain = request.DataStore.GetValue("ImpersonationDomain") ?? Environment.GetEnvironmentVariable("USERDOMAIN");
            string user = request.DataStore.GetValue("ImpersonationUsername") ?? Environment.GetEnvironmentVariable("USERNAME");

            string domainAccount = $"{NTHelper.CleanDomain(domain)}\\{NTHelper.CleanUsername(user)}";

            // This will throw an error if the permission cannot be granted
            NTPermissionUtility.SetRight(Environment.MachineName, domainAccount, NTPermissionUtility.LOGON_AS_BATCH_PERM, false);

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }
    }
}