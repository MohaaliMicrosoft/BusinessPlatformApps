using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.AzureCustom.Twitter
{
    [Export(typeof(IAction))]
    public class DeployTwitterFilesV2 : BaseAction
    {
        public override ActionResponse ExecuteAction(ActionRequest request)
        {
            string token = request.Message["Token"][0]["access_token"].ToString();
            string subscription = request.Message["SelectedSubscription"][0]["SubscriptionId"].ToString();
            string resourceGroup = request.Message["SelectedResourceGroup"][0].ToString();
            string sitename = request.Message["SiteName"][0].ToString();

            AzureHttpClient client = new AzureHttpClient(token, subscription, resourceGroup);
            HttpResponseMessage publishxml = client.ExecuteWithSubscriptionAndResourceGroupAsync(HttpMethod.Post, "/providers/Microsoft.Web/sites/" + sitename + "/publishxml", "2015-02-01", string.Empty);
            var publishxmlfile = publishxml.Content.ReadAsStringAsync().Result;

            XDocument doc = XDocument.Parse(publishxmlfile);
            XElement xElement = doc.Element("publishData");

            if (xElement != null)
            {
                var publishProfiles = xElement.Elements("publishProfile");
                var profile = publishProfiles.SingleOrDefault(p => p.Attribute("publishMethod").Value == "FTP");

                if (profile == null)
                {
                    return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromStringValue(publishxmlfile), null, DefaultErrorCodes.DefaultErrorCode,"Unable to find FTP profile");
                }

                var ftpServer = profile.Attribute("publishUrl").Value;
                var username = profile.Attribute("userName").Value;
                var password = profile.Attribute("userPWD").Value;

                return new ActionResponse(ActionStatus.Success, JsonUtility.GetJObjectFromStringValue(publishxmlfile));
            }

            return new ActionResponse(ActionStatus.Failure, JsonUtility.GetJObjectFromStringValue(publishxmlfile), null, DefaultErrorCodes.DefaultErrorCode, "Unable to find FTP profile");
        }
    }
}