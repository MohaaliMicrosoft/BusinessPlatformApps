using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.Helpers;

namespace Microsoft.Deployment.Common.ActionModel
{
    public class ActionResponseExceptionDetail
    {
        public Exception ExceptionCaught;

        public string LogLocation { get; set; }

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
    }
}
