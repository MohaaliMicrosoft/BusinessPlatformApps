using Microsoft.Deployment.Actions.Test.TestHelpers;
using Microsoft.Deployment.Common;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Deployment.Actions.Test.ActionsTest
{
    [TestClass]
    public class CDMTests
    {
        [TestMethod]
        public async Task GetCDMNamespaces()
        {
            //Get Token
            var datastore = await AAD.GetTokenWithDataStore();
            var result = await TestHarness.ExecuteActionAsync("Microsoft-GetAzureSubscriptions", datastore);
            Assert.IsTrue(result.IsSuccess);
            var responseBody = JObject.FromObject(result.Body);
        }
        [TestMethod]
        public async Task GetObjID()
        {
            //Get Token
            var dataStore = await AAD.GetUserTokenFromPopup();
            var result = await TestHarness.ExecuteActionAsync("Microsoft-GetAzureSubscriptions", dataStore);
            Assert.IsTrue(result.IsSuccess);
            var responseBody = JObject.FromObject(result.Body);
            var subscriptionId = responseBody["value"][0]["SubscriptionId"].ToString();

            dataStore.AddToDataStore("SubscriptionId", subscriptionId, DataStoreType.Private);
            var res = TestHarness.ExecuteAction("Microsoft-GetObjID", dataStore);
            Assert.IsTrue(res.Status == ActionStatus.Success);
        }

        [TestMethod]
        public async Task GetEnvironID()
        {
            //Get Token
            var dataStore = await AAD.GetUserTokenFromPopup();
            var result = await TestHarness.ExecuteActionAsync("Microsoft-GetAzureSubscriptions", dataStore);
            Assert.IsTrue(result.IsSuccess);
            var responseBody = JObject.FromObject(result.Body);
            var subscriptionId = responseBody["value"][0]["SubscriptionId"].ToString();

            dataStore.AddToDataStore("SubscriptionId", subscriptionId, DataStoreType.Private);
            var objIdResponse = TestHarness.ExecuteAction("Microsoft-GetObjID", dataStore);

            var environIdResponse = TestHarness.ExecuteAction("Microsoft-GetEnvironID", dataStore);


            Assert.IsTrue(environIdResponse.Status == ActionStatus.Success);
        }

        [TestMethod]
        public async Task CheckCDMEntities()
        {
            //Get Token
            var dataStore = await AAD.GetUserTokenFromPopup();
            var result = await TestHarness.ExecuteActionAsync("Microsoft-GetAzureSubscriptions", dataStore);
            Assert.IsTrue(result.IsSuccess);
            var responseBody = JObject.FromObject(result.Body);
            var subscriptionId = responseBody["value"][0]["SubscriptionId"].ToString();

            dataStore.AddToDataStore("SubscriptionId", subscriptionId, DataStoreType.Private);

            var objIdResponse = TestHarness.ExecuteAction("Microsoft-GetObjID", dataStore);
            var environIdResponse = TestHarness.ExecuteAction("Microsoft-GetEnvironID", dataStore);
            var getCDMEntityResponse = TestHarness.ExecuteAction("Microsoft-CheckCDMEntities", dataStore);


            Assert.IsTrue(getCDMEntityResponse.Status == ActionStatus.Success);
        }
    }
}
