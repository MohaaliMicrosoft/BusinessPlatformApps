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
        public void CreateTaskWithCorrectCredentialsSuccess()
        {
            var dataStore = new DataStore();

            dataStore.AddToDataStore("ImpersonationUsername", "TestUser");
            dataStore.AddToDataStore("ImpersonationPassword", "TestP@ssw0rd");
            dataStore.AddToDataStore("ImpersonationDomain", "\\");

            dataStore.AddToDataStore("TaskDescription", "Test Task Description");
            dataStore.AddToDataStore("TaskFile", "test-file.ps1");
            dataStore.AddToDataStore("TaskLocation", "Business Platform Solution Templates\\Test\\");
            dataStore.AddToDataStore("TaskName", "Test Task Name");
            dataStore.AddToDataStore("TaskParameters", "-ExecutionPolicy Bypass");
            dataStore.AddToDataStore("TaskProgram", "powershell");
            dataStore.AddToDataStore("TaskStartTime", "2:00");

            var result = TestHarness.ExecuteAction("Microsoft-CreateTask", dataStore);
            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void RemoveTaskSuccess()
        {
            var dataStore = new DataStore();
            dataStore.AddToDataStore("TaskName", "Test Task Name");

            var result = TestHarness.ExecuteAction("Microsoft-RemoveTask", dataStore);
            Assert.IsTrue(result.IsSuccess);
        }

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
}
