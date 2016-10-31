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
            dataStore.AddToDataStore("Salesforce", "SalesforceUser", "", DataStoreType.Any);
            dataStore.AddToDataStore("Salesforce", "SalesforcePassword", "", DataStoreType.Any);
            dataStore.AddToDataStore("Salesforce", "SalesforceToken", "", DataStoreType.Any);
            dataStore.AddToDataStore("Salesforce", "SalesforceUrl", "https://login.salesforce.com/", DataStoreType.Any);

            var result = TestHarness.ExecuteAction("Microsoft-ValidateSalesforceCredentials", dataStore);
            Assert.IsTrue(result.Status == ActionStatus.Success);
        }
    }
}
