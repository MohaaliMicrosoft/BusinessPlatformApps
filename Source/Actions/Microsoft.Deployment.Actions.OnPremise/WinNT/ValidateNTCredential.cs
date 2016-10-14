using System;
using System.ComponentModel.Composition;
using System.DirectoryServices.AccountManagement;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.OnPremise.WinNT
{
    [Export(typeof(IAction))]
    public class ValidateNtCredential : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string domain = NTHelper.CleanDomain(request.Message["ImpersonationDomain"][0].ToString());
            string user = NTHelper.CleanUsername(request.Message["ImpersonationUsername"][0].ToString());
            string password = request.Message["ImpersonationPassword"][0].ToString();

            bool isValid;
            ContextType context = Environment.MachineName.EqualsIgnoreCase(domain) ? ContextType.Machine : ContextType.Domain;

            using (PrincipalContext pc = new PrincipalContext(context, domain))
            {
                // validate the credentials
                isValid = pc.ValidateCredentials(user, password, ContextOptions.Negotiate);
            }

            return isValid
                ? new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject())
                : new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "IncorrectNTCredentials");
        }
    }
}