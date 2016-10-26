using System.Collections.Generic;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Controller;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.ActionModel
{
    public class ActionRequest
    {
        public CommonControllerModel ControllerModel { get; set; }
        public UserInfo Info { get; set; }
        public Logger Logger { get; set; }
        public DataStore DataStore { get; set; }


        public ActionRequest()
        {
        }
    }
}