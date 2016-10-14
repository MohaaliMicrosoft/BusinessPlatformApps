using System.ComponentModel.Composition;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.ErrorCode;
using Newtonsoft.Json.Linq;
using Simple.CredentialManager;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.Bpst.Actions.OnPremiseActions.CredentialManager
{
    [Export(typeof(IAction))]
    public class CredentialManagerRead : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string targetName = request.Message["CredentialTarget"].ToString();
            string userName = request.Message["CredentialUsername"].ToString();

            Credential c = new Credential(userName)
            {
                Target = targetName,
                Type = CredentialType.Generic
            };

            if (c.Load())
            {
                CredentialManagerEntry cmEntry = new CredentialManagerEntry { Username = userName, Password = c.Password, Target = c.Target };
                return new ActionResponse(ActionStatus.Success, JObject.FromObject(cmEntry));
            }

            return new ActionResponse(ActionStatus.Failure, new JObject(), new Win32Exception(Marshal.GetLastWin32Error()), "CredMgrReadError");
        }
    }
}