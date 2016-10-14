using System;
using System.ComponentModel.Composition;
using System.Data;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.OnPremise
{
    [Export(typeof(IAction))]
    public class GetDataPullStatus : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            ActionResponse response;

            bool isWaiting = request.Message["IsWaiting"] == null
                ? false
                : bool.Parse(request.Message["IsWaiting"].ToString());

            int sqlIndex = int.Parse(request.Message["SqlServerIndex"].ToString());

            string connectionString = request.Message["SqlConnectionString"][sqlIndex].ToString(); // Must specify Initial Catalog
            string finishedActionName = request.Message["FinishedActionName"] == null
                ? null
                : request.Message["FinishedActionName"].ToString();
            string targetSchema = request.Message["TargetSchema"].ToString(); // Specifies the schema used by the template

            string query = $"[{targetSchema}].sp_get_replication_counts";

            DataTable recordCounts;

            try
            {
                recordCounts = SqlUtility.InvokeStoredProcedure(connectionString, query, null);
            }
            catch
            {
                // It's ok for this to fail, we'll just return an empty table
                recordCounts = new DataTable();
            }

            bool isPullingData = false;

            if (isWaiting)
            {
                foreach (DataRow row in recordCounts.Rows)
                {
                    isPullingData = Convert.ToInt64(row["Count"]) > 0;
                    if (isPullingData)
                        break;
                }

                response = isPullingData
                    ? new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject())
                    : new ActionResponse(ActionStatus.BatchNoState, JsonUtility.GetEmptyJObject());
            }
            else
            {
                response = new ActionResponse(ActionStatus.Success, "{isFinished:false,status:" + JsonUtility.Serialize(recordCounts) + "}");
            }

            if (string.IsNullOrEmpty(finishedActionName))
                return response;

            ActionResponse finishedResponse = RequestUtility.CallAction(request, finishedActionName);

            var content = finishedResponse.Response["value"]?.ToString();

            if ((isPullingData && finishedResponse.Status != ActionStatus.Failure) || finishedResponse.Status == ActionStatus.Success)
            {
                var resp = new ActionResponse();
                if (!string.IsNullOrEmpty(content))
                {
                    resp = new ActionResponse(ActionStatus.Success,
                        "{isFinished:true,FinishedActionName:\"" +
                        finishedActionName +
                        "\",TargetSchema:\"" + targetSchema + 
                        "\",status:" + JsonUtility.Serialize(recordCounts) +
                        ", slices:" + finishedResponse.Response["value"].ToString() + "}");
                }
                else
                {
                    resp = new ActionResponse(ActionStatus.Success, "{isFinished:true, status:" + JsonUtility.Serialize(recordCounts) + "}");
                }
                return resp;
            }

            if (finishedResponse.Status == ActionStatus.BatchNoState || finishedResponse.Status == ActionStatus.BatchWithState)
            {
                var resp = new ActionResponse();
                if (!string.IsNullOrEmpty(content))
                {
                    resp = new ActionResponse(ActionStatus.Success,
                    "{isFinished:false,FinishedActionName:\"" +
                    finishedActionName +
                     "\",TargetSchema:\"" + targetSchema + 
                     "\",status:" + JsonUtility.Serialize(recordCounts) +
                    ", slices:" + finishedResponse.Response["value"].ToString() + "}");
                }
                else
                {
                    resp = new ActionResponse(ActionStatus.Success, "{isFinished:false, status:" + JsonUtility.Serialize(recordCounts) + "}");
                }
                return resp;
            }

            return finishedResponse;
        }
    }
}