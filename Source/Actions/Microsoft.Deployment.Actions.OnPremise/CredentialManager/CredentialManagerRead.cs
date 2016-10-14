using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.Deployment.Common.Actions;
using Newtonsoft.Json.Linq;
using Simple.CredentialManager;

namespace Microsoft.Deployment.Actions.OnPremise.CredentialManager
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