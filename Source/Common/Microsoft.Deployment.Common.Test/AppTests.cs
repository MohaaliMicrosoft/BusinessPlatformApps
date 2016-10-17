using Microsoft.Deployment.Common.AppLoad;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Deployment.Common.Test
{
    [TestClass]
    public class AppTests
    {
        [TestMethod]
        public void GetApps()
        {
            AppFactory appFactory = new AppFactory();
            Assert.IsTrue(appFactory.Apps.Count > 0);
        }
    }
}
