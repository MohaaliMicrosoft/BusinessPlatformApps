using System;
using System.Collections.Generic;
using Microsoft.Deployment.Common.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.TestHarness
{
    public class DataStoreMock
    {
        private Dictionary<string, Dictionary<string, object>> dataStore;

        public DataStoreMock()
        {
            this.dataStore = new Dictionary<string, Dictionary<string, object>>();
        }
        public void AddItemToDataStore(string route, string key, object value)
        {
            if (!this.dataStore.ContainsKey(route))
            {
                this.dataStore.Add(route, new Dictionary<string, object>());
            }

            if (this.dataStore[route].ContainsKey(key))
            {
                this.dataStore[route][key] = value;
            }
            else
            {
                this.dataStore[route].Add(key, value);
            }
        }

        public void AddObjectToDataStore(string route, object obj)
        {
            var convertedObj = JsonUtility.GetJObjectFromObject(obj);

            foreach (var item in convertedObj.Properties())
            {
                this.AddItemToDataStore(route, item.Name, item.Value);
            }
        }

        public JObject GetDataStore()
        {
            JObject jsonObj = new JObject();

            foreach (var topLevelItems in this.dataStore.Keys)
            {
                foreach (var item in this.dataStore[topLevelItems].Keys)
                {
                    if (jsonObj.SelectToken(item).IsNullOrEmpty())
                    {
                        JArray array = new JArray();
                        AddValueCompletely(topLevelItems, item, array);
                        jsonObj.Add(new JProperty(item, array));
                    }
                    else
                    {
                        var array = (JArray) jsonObj.SelectToken(item);
                        AddValueCompletely(topLevelItems, item, array);
                    }
                }
            }

            return jsonObj;
        }


        private void AddValueCompletely(string topLevelItems, string item, JArray array)
        {
            if (this.dataStore[topLevelItems][item] is JArray)
            {
                foreach (var o in (JArray)this.dataStore[topLevelItems][item])
                {
                    var objToAdd = o;
                   
                    array.Add(objToAdd);
                }
            }
            else
            {
                AddValue(topLevelItems, item, array);
            }
        }

        private void AddValue(string topLevelItems, string item, JArray array)
        {


            if (this.dataStore[topLevelItems][item].GetType().IsGenericType)
            {
                array.Add(new JRaw(this.dataStore[topLevelItems][item]));
            }
            else
            {
                object o = this.dataStore[topLevelItems][item];
                try
                {
                    o = JsonUtility.GetJObjectFromObject((this.dataStore[topLevelItems][item]));
                }
                catch (Exception)
                {
                }

                array.Add(o);
            }
        }
    }
}
