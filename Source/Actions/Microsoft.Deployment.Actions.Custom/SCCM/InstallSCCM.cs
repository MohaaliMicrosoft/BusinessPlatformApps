using System.ComponentModel.Composition;
using System.IO;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.Custom.SCCM
{
    [Export(typeof(IAction))]
    public class InstallSCCM : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string sccmPath = FileUtility.GetLocalTemplatePath(request.TemplateName);

            if (!Directory.Exists(sccmPath))
            {
                Directory.CreateDirectory(sccmPath);
            }

            string[] files = Directory.GetFiles(Path.Combine(request.TemplatePath, RESOURCE_PATH));

            // Copy the files and overwrite destination files if they already exist.
            foreach (string s in files)
            {
                // Use static Path methods to extract only the file name from the path.
                string fileName = Path.GetFileName(s);
                string destFile = Path.Combine(sccmPath, fileName);
                File.Copy(s, destFile, true);
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }

        public const string RESOURCE_PATH = @"Service\Resources\Scripts\";
    }
}