using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using Microsoft.Bpst.Shared.Actions;
using Microsoft.Bpst.Shared.Helpers;

namespace Microsoft.Bpst.Actions.CommonActions.Test
{
    [Export(typeof(IAction))]
    public class TestDataStore : BaseAction
    {
        public override  ActionResponse ExecuteAction(ActionRequest request)
        {
            int index = 0;
            var currentIndexes = request.Message["CurrentIndex"];
            if (!currentIndexes.IsNullOrEmpty())
            {
                index = int.Parse(currentIndexes[currentIndexes.Count()-1].ToString());
            }

            int batchSize = 500;

            dynamic response = new ExpandoObject();
            response.CurrentIndex = index + batchSize;

            if (index > 1000)
            {
                return new ActionResponse(ActionStatus.Success, "{}");
            }
            else
            {
                return new ActionResponse(ActionStatus.BatchWithState, response);
            }

        }
    }
}
