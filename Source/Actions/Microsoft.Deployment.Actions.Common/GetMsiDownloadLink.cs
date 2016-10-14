using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.ErrorCode;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.CommonActions
{
    [Export(typeof(IAction))]
    public class GetMsiDownloadLink : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var msi = System.IO.Directory.GetFiles(request.TemplatePath, "*.exe");
            if (msi.Length == 1)
            {
                var file = new FileInfo(msi.First());
                string serverPath = request.SiteTemplatePath + $"/{file.Name}";
                return new ActionResponse(ActionStatus.Success, JsonUtility.GetJObjectFromStringValue(serverPath));
            }

            try
            {
                string evaluationEmail = request.Message["EvaluationEmail"] == null
                    ? string.Empty
                    : request.Message["EvaluationEmail"][0].ToString();
                if (!string.IsNullOrEmpty(evaluationEmail))
                {
                    request.Logger.LogCustomProperty("Email", evaluationEmail);
                }
            }
            catch
            {
                // Logging email failed
            }

            return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), null, DefaultErrorCodes.DefaultErrorCode, "Msi count:" + msi.Length);
        }
    }
}