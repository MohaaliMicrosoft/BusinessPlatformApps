using System;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.CommonActions.Test
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
