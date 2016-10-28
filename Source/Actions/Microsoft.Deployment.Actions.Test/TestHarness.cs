﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.AppLoad;
using Microsoft.Deployment.Common.Controller;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Deployment.Actions.Test
{
    [TestClass]
    public class TestHarness
    {
        private static CommonController Controller { get; set; }

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            AppFactory factory = new AppFactory();
            CommonControllerModel model = new CommonControllerModel()
            {
                AppFactory = factory
            };

            Controller = new CommonController(model);
        }

        public static ActionResponse ExecuteAction(string actionName, DataStore datastore)
        {
            UserInfo info = new UserInfo();
            info.ActionName = actionName;
            info.AppName = "TestApp";
            return Controller.ExecuteAction(info, new ActionRequest() { DataStore = datastore }).Result;
        }
    }
}