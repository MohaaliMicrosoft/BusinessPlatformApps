using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.AppLoad;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Deployment.Common.Test
{
    [TestClass]
    public class TemplateParserTests
    {
        [TestMethod]
        public void GetApps()
        {
            AppFactory appFactory = new AppFactory();
            Assert.IsTrue(appFactory.Apps.Count > 0);
        }
    }
}
