using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;

namespace Microsoft.Deployment.Common.Test.DummyActions
{
    [Export(typeof(IAction))]

    public class TestAction : BaseAction
    {
        public override Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
