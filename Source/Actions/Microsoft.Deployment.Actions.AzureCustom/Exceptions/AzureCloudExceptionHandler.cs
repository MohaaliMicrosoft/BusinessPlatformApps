using System;
using System.ComponentModel.Composition;
using Hyak.Common;
using Microsoft.Deployment.Actions.AzureCustom.AzureToken;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Actions.AzureCustom.Exceptions
{
    [Export(typeof(IActionExceptionHandler))]
    public class AzureCloudExceptionHandler : IActionExceptionHandler
    {
        public Type ExceptionExpected { get; } = typeof(CloudException);

        public ActionResponse HandleException(ActionRequest request, Exception exception)
        {
            CloudException cloudException = exception as CloudException;
            if (cloudException != null)

                // This can be fired if by a split second the token expires during the call
                if (cloudException.Message.StartsWith("ExpiredAuthenticationToken"))
                {
                    RefreshAzureToken tokenRefresh = new RefreshAzureToken();
                    var tokenRefreshResponse = tokenRefresh.ExecuteAction(request);
                    if (tokenRefreshResponse.Status == ActionStatus.Success)
                    {
                        request.Message["Token"] = tokenRefreshResponse.Response["Token"];
                    }
                    return new ActionResponse(ActionStatus.Retry, JsonUtility.GetEmptyJObject(), exception, AzureErrorCodes.TokenExpired);
                }

            return new ActionResponse(ActionStatus.UnhandledException);
        }
    }
}
