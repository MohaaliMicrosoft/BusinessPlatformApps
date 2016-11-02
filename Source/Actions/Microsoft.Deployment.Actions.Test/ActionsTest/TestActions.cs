using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Deployment.Actions.Test.CommonActions
{
    [TestClass]
    public class TestActions
    {
        [TestMethod]
        public void RunSimpleTestActionTest()
        {
            var result = TestHarness.ExecuteAction("Microsoft-Test", new DataStore());
            Assert.IsTrue(result.Status == ActionStatus.Success);
        }

        [TestMethod]
        public void RunSalesforceCredentialValidation()
        {
            DataStore dataStore = new DataStore();
            dataStore.AddToDataStore("SalesforceUser", "cat@catinc.com", DataStoreType.Public);
            dataStore.AddToDataStore("SalesforcePassword", "P@ssword.123", DataStoreType.Private);
            dataStore.AddToDataStore("SalesforceToken", "5jxocWhvDfVxJd0SoOj3zM38", DataStoreType.Private);
            dataStore.AddToDataStore("SalesforceUrl", "https://login.salesforce.com/", DataStoreType.Public);

            var result = TestHarness.ExecuteAction("Microsoft-ValidateSalesforceCredentials", dataStore);
            Assert.IsTrue(result.Status == ActionStatus.Success);
        }
    }
}
