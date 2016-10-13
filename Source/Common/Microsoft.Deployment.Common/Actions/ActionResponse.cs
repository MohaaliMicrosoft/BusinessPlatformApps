using System;
using Microsoft.Deployment.Common.ErrorCode;
using Microsoft.Deployment.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.Actions
{
    public class ActionResponse
    {
        // Status of the request
        [JsonConverter(typeof(StringEnumConverterLower))]
        public ActionStatus Status;

        public JObject Response;

        public Exception ExceptionCaught;

        public string LogLocation { get; set; }

        public string Message { get; set; }

        public string FriendlyMessageCode { get; set; }

        public string FriendlyErrorMessage
        {
            get
            {
                if (this.FriendlyMessageCode == null)
                {
                    return string.Empty;
                }

                 return ErrorUtility.GetErrorCode(this.FriendlyMessageCode);
            }
        }

        public string AdditionalDetailsErrorMessage { get; set; }


        // Used to Serialize and Deserialize
        public ActionResponse()
        {

        }

        public ActionResponse(ActionStatus status) : this(status, JsonUtility.GetEmptyJObject())
        {
        }

        public ActionResponse(ActionStatus status, JObject response)
        {
            if (status == ActionStatus.Failure || status == ActionStatus.FailureExpected)
            {
                this.FriendlyMessageCode = DefaultErrorCodes.DefaultErrorCode;
            }
            this.Status = status;
            this.Response = response;
        }

        public ActionResponse(ActionStatus status, string response)
        {
            JObject obj = new JObject();

            if (status == ActionStatus.Failure)
            {
                this.FriendlyMessageCode = DefaultErrorCodes.DefaultErrorCode;
            }
            if (!string.IsNullOrEmpty(response))
            {
                obj = JsonUtility.GetJsonObjectFromJsonString(response);
            }

            this.Status = status;
            this.Response = obj;
        }

        public ActionResponse(ActionStatus status, object response)
        {
            if (status == ActionStatus.Failure)
            {
                this.FriendlyMessageCode = DefaultErrorCodes.DefaultErrorCode;
            }

            this.Status = status;
            this.Response = JsonUtility.GetJObjectFromObject(response);
        }

        public ActionResponse(ActionStatus status, object response, string friendlyMessageCode)
        {
            if (status == ActionStatus.Failure)
            {
                this.FriendlyMessageCode = friendlyMessageCode;
            }

            this.Status = status;
            this.Response = JsonUtility.GetJObjectFromObject(response);
        }

        public ActionResponse(ActionStatus status, JObject response, Exception exception,
            string friendlyMessageCode, string additionaldetails)
        {
            this.Status = status;
            this.Response = response;
            this.ExceptionCaught = exception;
            this.FriendlyMessageCode = friendlyMessageCode;
            this.AdditionalDetailsErrorMessage = additionaldetails;
        }

        public ActionResponse(ActionStatus status, JObject response, Exception exception,
            string friendlyMessageCode)
        {
            this.Status = status;
            this.Response = response;
            this.ExceptionCaught = exception;
            this.FriendlyMessageCode = friendlyMessageCode;
            this.AdditionalDetailsErrorMessage = GetInnerExceptionText(exception);
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