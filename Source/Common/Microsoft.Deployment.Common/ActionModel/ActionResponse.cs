using System;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.ActionModel
{
    public class ActionResponse
    {
        // Status of the request
        [JsonConverter(typeof(StringEnumConverterLower))]
        public ActionStatus Status { get; set; }

        public object Body { get; private set; }

        public bool IsPrivate { get; set; }

        public DataStore DataStore { get; set; }

        public ActionResponseExceptionDetail ExceptionDetail { get; set; }

        // Used to Serialize and Deserialize for testing purposes only
        public ActionResponse()
        {
        }

        public ActionResponse(ActionStatus status) : this(status, new object(), false)
        {
        }

        public ActionResponse(ActionStatus status, object response, bool isPrivate)
        {
            this.IsPrivate = isPrivate;
            if (status == ActionStatus.Failure || status == ActionStatus.FailureExpected)
            {
                this.ExceptionDetail.FriendlyMessageCode = DefaultErrorCodes.DefaultErrorCode;
            }
            this.Status = status;
            this.Body = response;
        }

        public ActionResponse(ActionStatus status, string response, bool isPrivate)
        {
            JObject obj = new JObject();

            this.IsPrivate = isPrivate;
            if (status == ActionStatus.Failure)
            {
                this.ExceptionDetail.FriendlyMessageCode = DefaultErrorCodes.DefaultErrorCode;
            }
            if (!string.IsNullOrEmpty(response))
            {
                obj = JsonUtility.GetJsonObjectFromJsonString(response);
            }

            this.Status = status;
            this.Body = obj;
        }

        public ActionResponse(ActionStatus status, object response, string friendlyMessageCode)
        {
            if (status == ActionStatus.Failure)
            {
                this.ExceptionDetail.FriendlyMessageCode = friendlyMessageCode;
            }

            this.Status = status;
            this.Body = JsonUtility.GetJObjectFromObject(response);
        }

        public ActionResponse(ActionStatus status, object response, Exception exception,
            string friendlyMessageCode, string additionaldetails)
        {
            this.Status = status;
            this.Body = response;
            this.ExceptionDetail.ExceptionCaught = exception;
            this.ExceptionDetail.FriendlyMessageCode = friendlyMessageCode;
            this.ExceptionDetail.AdditionalDetailsErrorMessage = additionaldetails;
        }

        public ActionResponse(ActionStatus status, object response, Exception exception,
            string friendlyMessageCode)
        {
            this.Status = status;
            this.Body = response;
            this.ExceptionDetail.ExceptionCaught = exception;
            this.ExceptionDetail.FriendlyMessageCode = friendlyMessageCode;
            this.ExceptionDetail.AdditionalDetailsErrorMessage = GetInnerExceptionText(exception);
        }

        private static string GetInnerExceptionText(Exception exception)
        {
            if (exception == null)
                return string.Empty;

            Exception loopException = exception;
            while (loopException.InnerException != null)
            {
                loopException = loopException.InnerException;
            }

            return loopException.Message;
        }
    }
}