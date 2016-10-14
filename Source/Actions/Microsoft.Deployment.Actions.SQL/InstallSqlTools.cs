﻿using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;

using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.DatabaseActions
{
    [Export(typeof(IAction))]
    public class InstallSqlTools : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            ActionResponse response = new ActionResponse(ActionStatus.Success, "");

            try
            {
                string msiLocationName = Path.Combine(request.TemplatePath, "Service\\Resources").Replace('/', '\\');
                string[] installSequence =
                {
                $"/i \"{Path.Combine(msiLocationName, "msodbcsql.msi")}\" /quiet /qn /promptrestart IACCEPTMSODBCSQLLICENSETERMS=YES",
                $"/i \"{Path.Combine(msiLocationName, "MsSqlCmdLnUtils.msi")}\" /quiet /qn /promptrestart IACCEPTMSSQLCMDLNUTILSLICENSETERMS=YES"
                };

                using (Process p = new Process())
                {
                    for (int i = 0; i < installSequence.Length; i++)
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo("msiexec.exe", installSequence[i]) { WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = msiLocationName };
                        p.StartInfo = startInfo;
                        if (p.Start())
                        {
                            p.WaitForExit();
                            if (p.ExitCode == 0 || p.ExitCode == 1641 || p.ExitCode == 3010) continue;
                            response = new ActionResponse(ActionStatus.Failure, installSequence[i] + " failed to install.");
                            break;
                        }

                        response = new ActionResponse(ActionStatus.Failure, "MSIEXEC failed to execute " + installSequence[i]);
                        break;
                    }
                }
            }
            catch
            {
                response = new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "InstallSQLToolsFailed");
            }

            return response;
        }
    }
}