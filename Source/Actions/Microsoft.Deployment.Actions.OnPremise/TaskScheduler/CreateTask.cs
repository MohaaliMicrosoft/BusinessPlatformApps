using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Security.Principal;
using System.Text.RegularExpressions;

using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Helpers;
using Microsoft.Win32.TaskScheduler;

namespace Microsoft.Bpst.Actions.OnPremiseActions.TaskScheduler
{
    [Export(typeof(IAction))]
    public class CreateTask : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var taskDescription = request.Message["TaskDescription"][0].ToString();
            var taskFile = request.Message["TaskFile"][0].ToString();
            var taskName = request.Message["TaskName"][0].ToString();
            var taskParameters = request.Message["TaskParameters"][0].ToString();
            var taskProgram = request.Message["TaskProgram"][0].ToString();
            var taskStartTime = request.Message["TaskStartTime"][0].ToString();

            var taskUsername = request.Message["ImpersonationUsername"] == null || string.IsNullOrEmpty(request.Message["ImpersonationUsername"][0].ToString())
                ? WindowsIdentity.GetCurrent().Name
                : NTHelper.CleanDomain(request.Message["ImpersonationDomain"][0].ToString()) + "\\" + NTHelper.CleanUsername(request.Message["ImpersonationUsername"][0].ToString());
            var taskPassword = request.Message["ImpersonationPassword"] == null || string.IsNullOrEmpty(request.Message["ImpersonationPassword"][0].ToString())
                ? null
                : request.Message["ImpersonationPassword"][0].ToString();

            if (taskPassword == null)
                return new ActionResponse(ActionStatus.Failure, JsonUtility.GetEmptyJObject(), "CreateTaskPasswordMissing");

            string workingDirectory = request.Message["TaskDirectory"] == null
                ? FileUtility.GetLocalTemplatePath(request.TemplateName)
                : FileUtility.GetLocalPath(request.Message["TaskDirectory"][0].ToString());

            bool isPowerShell = taskProgram.EqualsIgnoreCase("powershell");

            using (TaskService ts = new TaskService())
            {
                TaskCollection tasks = ts.RootFolder.GetTasks(new Regex(taskName));
                foreach (Task task in tasks)
                {
                    if (task.Name.EqualsIgnoreCase(taskName))
                    {
                        ts.RootFolder.DeleteTask(taskName);
                    }
                }

                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = taskDescription;
                td.Settings.Compatibility = TaskCompatibility.V2_1;
                td.RegistrationInfo.Author = taskUsername;
                td.Principal.RunLevel = TaskRunLevel.LUA;
                td.Settings.StartWhenAvailable = true;
                td.Settings.RestartCount = 3;
                td.Settings.RestartInterval = TimeSpan.FromMinutes(3);
                td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;

                td.Triggers.Add(new DailyTrigger
                {
                    DaysInterval = 1,
                    StartBoundary = DateTime.Parse(taskStartTime)
                });

                string optionalArguments = string.Empty;

                if (isPowerShell)
                    optionalArguments = Path.Combine(workingDirectory, taskFile);
                else
                    taskProgram = taskFile;

                if (isPowerShell)
                {
                    optionalArguments = $"-ExecutionPolicy Bypass -File \"{optionalArguments}\"";
                }

                if (!string.IsNullOrEmpty(taskParameters))
                {
                    optionalArguments += " " + taskParameters;
                }

                td.Actions.Add(new ExecAction(taskProgram, optionalArguments, workingDirectory));
                ts.RootFolder.RegisterTaskDefinition(taskName, td, TaskCreation.CreateOrUpdate, taskUsername, taskPassword, TaskLogonType.Password, null);
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }
    }
}