using System;
using System.ComponentModel.Composition;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.SalesforceADF
{
    [Export(typeof(IAction))]
    class ADFSliceStatus : BaseAction
    {
        private string apiVersion = "2015-10-01";
        private string getDatasetRelativeUrl = "providers/Microsoft.DataFactory/datafactories/{0}/datasets";
        private string getSlicesRelativeUrl = "/slices?start={0}&end={1}";

        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            var token = request.Message["Token"][0]["access_token"].ToString();
            var subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            var resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            var dataFactory = resourceGroup + "SalesforceCopyFactory";

            var url = string.Format(getDatasetRelativeUrl, dataFactory);

            DataTable table = new DataTable();
            table.Columns.Add("Dataset");
            table.Columns.Add("Start");
            table.Columns.Add("End");
            table.Columns.Add("Status");

            var client = new AzureHttpClient(token, subscription, resourceGroup);

            var connection = client.ExecuteWithSubscriptionAndResourceGroupAsync(HttpMethod.Get,
                  url, apiVersion, string.Empty);

            if (!connection.IsSuccessStatusCode)
            {
                return new ActionResponse(ActionStatus.FailureExpected,
                    JsonUtility.GetJObjectFromJsonString(connection.Content.ReadAsStringAsync().Result), null, DefaultErrorCodes.DefaultErrorCode,
                    "Failed to get consent");
            }

            var connectionData = JsonUtility.GetJObjectFromJsonString(connection.Content.ReadAsStringAsync().Result);

            if (connectionData != null)
            {
                foreach (var dataset in connectionData["value"])
                {
                    var nameParts = dataset["name"].ToString().Split('_');
                    if (nameParts[0] == "PreDeployment" && nameParts[2] == "Output")
                    {
                        var sliceRelativeUrl = string.Concat(url, '/', dataset["name"].ToString());
                        sliceRelativeUrl.Remove(0, 1);
                        var sliceConnection = client.ExecuteWithSubscriptionAndResourceGroupAsync(HttpMethod.Get,
                            sliceRelativeUrl +
                            string.Format(getSlicesRelativeUrl,
                            DateTime.UtcNow.AddYears(-3).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                            DateTime.UtcNow.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)),
                            apiVersion,
                            string.Empty
                            );

                        if (!sliceConnection.IsSuccessStatusCode)
                        {
                            return new ActionResponse(ActionStatus.FailureExpected,
                                JsonUtility.GetJObjectFromJsonString(connection.Content.ReadAsStringAsync().Result), null, DefaultErrorCodes.DefaultErrorCode,
                                "Failed to get consent");
                        }

                        var data = JsonUtility.GetJObjectFromJsonString(sliceConnection.Content.ReadAsStringAsync().Result);

                        if (data["value"].Count() > 2)
                        {
                            int numberOfSlices = data["value"].Count() - 2;

                            var lastSlice = data["value"][numberOfSlices];

                            table.Rows.Add(new[] { dataset["name"].ToString().Split('_')[1], lastSlice["start"].ToString(), lastSlice["end"].ToString(), lastSlice["status"].ToString() });
                        }
                        else
                        {
                            table.Rows.Add(new[] { dataset["name"].ToString().Split('_')[1], string.Empty, string.Empty, data["value"][0]["status"].ToString() });
                        }
                    }
                }
            }

            var ready = from DataRow row in table.Rows
                        where (string)row["Status"] != "Ready"
                        select (string)row["Dataset"];

            var response = JsonUtility.CreateJObjectWithValueFromObject(table);

            if (ready.Count() > 0)
            {
                return new ActionResponse(ActionStatus.BatchNoState, response);
            }
            else
            {
                return new ActionResponse(ActionStatus.Success, response);
            }
        }
    }
}
