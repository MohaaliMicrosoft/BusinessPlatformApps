using System;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.Common.Test
{
    [Export(typeof(IAction))]
    public class TestAction : BaseAction
    {
        public override  ActionResponse ExecuteAction(ActionRequest requestBody)
        {
            Thread.Sleep((int) TimeSpan.FromMinutes(6).TotalMilliseconds);
            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }
    }
}
