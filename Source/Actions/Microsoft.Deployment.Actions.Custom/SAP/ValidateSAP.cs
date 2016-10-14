using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.ErrorCode;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.CustomActions.SAP
{
    [Export(typeof(IAction))]
    public class ValidateSAP : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), EXECUTABLE_PATH);

            string exeDestination = Path.Combine(exePath, EXECUTABLE_NAME);
            string exeConfigDestination = Path.Combine(exePath, EXECUTABLE_NAME_CONFIG);

            if (!File.Exists(exeDestination) || !File.Exists(exeConfigDestination))
            {
                string exeSource = Path.Combine(request.TemplatePath, RESOURCE_PATH, EXECUTABLE_NAME);
                string exeConfigSource = Path.Combine(request.TemplatePath, RESOURCE_PATH, EXECUTABLE_NAME_CONFIG);

                (new FileInfo(exeDestination)).Directory.Create();

                File.Copy(exeSource, exeDestination, true);
                File.Copy(exeConfigSource, exeConfigDestination, true);
            }

            Process exeProcess = new Process();
            exeProcess.StartInfo.FileName = exeDestination;
            exeProcess.StartInfo.Arguments = "/s";
            exeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            exeProcess.Start();
            exeProcess.WaitForExit();

            if (exeProcess.ExitCode == 0)
            {
                return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
            }

            return new ActionResponse(ActionStatus.FailureExpected, JsonUtility.GetEmptyJObject(), null, DefaultErrorCodes.DefaultLoginFailed, exeProcess.ExitCode.ToString());
        }

        private const string EXECUTABLE_NAME = "Simplement.SolutionTemplate.AR.exe";
        private const string EXECUTABLE_NAME_CONFIG = "Simplement.SolutionTemplate.AR.exe.config";
        private const string EXECUTABLE_PATH = @"Simplement, Inc\Solution Template AR\";
        private const string RESOURCE_PATH = @"Service\Resources\";
    }
}