namespace Microsoft.Deployment.Actions.OnPremise.WinNT
{
    using Microsoft.Deployment.Common.ActionModel;
    using Microsoft.Deployment.Common.Actions;
    using System.ComponentModel.Composition;
    using System.Security.Principal;
    using System.Threading.Tasks;

    [Export(typeof(IAction))]
    public class GetCurrentUserAndDomain : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            return new ActionResponse(ActionStatus.Success, WindowsIdentity.GetCurrent().Name);
        }
    }
}