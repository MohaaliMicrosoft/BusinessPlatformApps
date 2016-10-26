using System.Collections.Generic;
using System.Linq;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.AppLoad;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

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

        [TestMethod]
        public void DataStoreRetrievalTest()
        {
            DataStore store = new DataStore();
            store.PrivateDataStore = new Dictionary<string,Dictionary<string,JToken>>();
            store.PublicDataStore = new Dictionary<string, Dictionary<string, JToken>>();

            store.PrivateDataStore.Add("TestRoute", new Dictionary<string, JToken>()
            {
                {"TestObject","TestValue" },
                {"TestObject2","TestValue2" },
                {"password","secret" }
            });

            store.PrivateDataStore.Add("TestRoute2", new Dictionary<string, JToken>()
            {
                {"TestObject","TestValue" },
                {"TestObject2","TestValue2" }
            });

            Assert.IsTrue(store.GetAllValues("TestObject").Count()  == 2);
            Assert.IsTrue(store.GetAllValues("TestObject2").Count() == 2);
            Assert.IsTrue(store.GetAllValues("password").Count() == 1);
            Assert.IsTrue(store.GetAllDataStoreItems("password").First().DataStoreType == DataStoreType.Private);
            Assert.IsTrue(store.GetAllDataStoreItems("password").First().ValueAsString == "secret");
            Assert.IsTrue(store.GetAllDataStoreItems("password").First().ToString() == "secret");

            var valueNotFound = store.GetValue("valuenothere");
            var valueNotFoundWithRoute = store.GetValue("routethere", "valuenothere");
            Assert.IsNull(valueNotFoundWithRoute);
            Assert.IsNull(valueNotFound);

            store.AddToDataStore("routethere", "valuenothere", "TestValue");
            valueNotFoundWithRoute = store.GetValue("routethere", "valuenothere");
            Assert.IsNotNull(valueNotFoundWithRoute);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(store.PrivateDataStore);

        }
    }
}
