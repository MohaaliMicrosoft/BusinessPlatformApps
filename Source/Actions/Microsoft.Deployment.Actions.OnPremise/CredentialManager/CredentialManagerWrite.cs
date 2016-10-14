using System.ComponentModel.Composition;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.ErrorCode;
using Newtonsoft.Json.Linq;
using Simple.CredentialManager;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Microsoft.Bpst.Actions.OnPremiseActions.CredentialManager
{
    [Export(typeof(IAction))]
    public class CredentialManagerWrite : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string targetName = request.Message["CredentialTarget"][0].ToString();
            string userName = request.Message["CredentialUsername"][0].ToString();
            string password = request.Message["CredentialPassword"][0].ToString();

            Credential c = new Credential(userName, password, targetName, CredentialType.Generic);
            c.PersistenceType = PersistenceType.LocalComputer;

            if (c.Save())
                return new ActionResponse(ActionStatus.Success, new JObject());
            else
                return new ActionResponse(ActionStatus.Failure, new JObject(), new Win32Exception(Marshal.GetLastWin32Error()), "CredMgrWriteError");
        }
    }
}