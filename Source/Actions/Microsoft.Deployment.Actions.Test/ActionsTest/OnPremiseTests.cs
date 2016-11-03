using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Deployment.Actions.Test.ActionsTest
{
    [TestClass]
    public class OnPremiseTests
    {
        [TestMethod]
        public async Task GetCurrentUserDoesNotFailTest()
        {
            var response = await TestHarness.ExecuteActionAsync("Microsoft-GetCurrentUserAndDomain", new DataStore());
            Assert.IsTrue(response.IsSuccess);
        }
    }
}
