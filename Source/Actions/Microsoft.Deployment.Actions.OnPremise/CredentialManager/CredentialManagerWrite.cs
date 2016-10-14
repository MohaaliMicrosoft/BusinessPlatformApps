using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.Deployment.Common.Actions;
using Newtonsoft.Json.Linq;
using Simple.CredentialManager;

namespace Microsoft.Deployment.Actions.OnPremise.CredentialManager
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