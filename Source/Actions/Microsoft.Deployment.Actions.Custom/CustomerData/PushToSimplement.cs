using System;
using System.ComponentModel.Composition;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Deployment.Actions.Custom.CustomerData
{
    [Export(typeof(IAction))]
    public class PushToSimplement : BaseAction
    {
        private const string connectionToSimplement = "?sv=2015-04-05&ss=t&srt=o&sp=rwlacu&se=2050-08-06T05:33:54Z&st=2016-08-04T21:33:54Z&spr=https&sig=3zLV0npI9j1Pgd7sdymi69cVEA0D%2FhfSLodYomCv5Vg%3D";

        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string fName  = request.Message["FirstName"][0].ToString();
            string lName  = request.Message["LastName"][0].ToString();
            string company = request.Message["CompanyName"][0].ToString();
            string email = request.Message["RowKey"][0].ToString();

            StorageCredentials sasCredentials = new StorageCredentials(connectionToSimplement);
            CloudTableClient tableClient = new CloudTableClient(new Uri("https://simpsolutiontemplate.table.core.windows.net/contact"), sasCredentials);
            CloudTable table = tableClient.GetTableReference("contact");

            CustomerInfoSimplement customerInfoSimplement = new CustomerInfoSimplement(email)
            {
                FirstName = fName,
                LastName = lName,
                CompanyName = company
            };

            // Cannot do insert_OR_replace since the policy won't allow it. Just do an insert
            try
            {
                table.Execute(TableOperation.Insert(customerInfoSimplement));
            }
            catch (Exception e)
            {
                // This call should never show an error to the user. We are mostly expecting a StorageException for duplicate values
                //if (e.Message.IndexOf("(409) Conflict", StringComparison.OrdinalIgnoreCase) == -1)
                    request.Logger.LogException(e);
            }

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetEmptyJObject());
        }
    }
}