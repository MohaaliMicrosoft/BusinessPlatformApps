using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.AzureCustom.Twitter
{
    [Export(typeof(IAction))]
    public class DeployTwitterTemplateFiles : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string token = request.Message["Token"][0]["access_token"].ToString();
            string subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            string resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            string sitename = request.Message["SiteName"][0].ToString();

            int index = 0;
            var currentIndexes = request.Message["CurrentIndex"];
            if (!currentIndexes.IsNullOrEmpty())
            {
                index = int.Parse(currentIndexes[currentIndexes.Count()-1].ToString());
            }

            int batchSize = 600;

            AzureHttpClient client = new AzureHttpClient(token, subscription, resourceGroup);
            HttpResponseMessage publishxml = client.ExecuteWithSubscriptionAndResourceGroupAsync(HttpMethod.Post, "/providers/Microsoft.Web/sites/" +
                sitename + "/publishxml", "2015-02-01", string.Empty);
            var publishxmlfile = publishxml.Content.ReadAsStringAsync().Result;

            XDocument doc = XDocument.Parse(publishxmlfile);
            XElement xElement = doc.Element("publishData");
            bool isFinished = false;

            if (xElement != null)
            {
                var publishProfiles = xElement.Elements("publishProfile");
                var profile = publishProfiles.SingleOrDefault(p => p.Attribute("publishMethod").Value == "FTP");

                if (profile == null)
                {
                    return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromStringValue(publishxmlfile), null, DefaultErrorCodes.DefaultErrorCode, "Unable to find FTP profile");
                }

                var ftpServer = profile.Attribute("publishUrl").Value;
                var username = profile.Attribute("userName").Value;
                var password = profile.Attribute("userPWD").Value;

                isFinished = FtpUtility.UploadAllViaZip(ftpServer, username, password, request.TemplatePath + "\\service\\data\\filestodeploy.zip", index, batchSize);

                if (isFinished)
                {
                    return new ActionResponse(ActionStatus.Success, "{}");
                }
                else
                {
                    dynamic responseToReturn = new ExpandoObject();
                    responseToReturn.CurrentIndex = index + batchSize;
                    return new ActionResponse(ActionStatus.BatchWithState, responseToReturn);
                }
            }

            return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromStringValue(publishxmlfile), null, DefaultErrorCodes.DefaultErrorCode, "Unable to find FTP profile");
        }
    }
}