using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.ActionModel
{
    /// <summary>
    /// This class stores both public and private details for the application
    /// This class should resemble the typescript to ensure consistency
    /// </summary>
    public class DataStore
    {
        public string CurrentRoutePage { get; set; }
        public string DeploymentIndex { get; set; }

        public string CurrentRoute => CurrentRoutePage + "-" + DeploymentIndex;

        public Dictionary<string,Dictionary<string,JToken>> PublicDataStore { get; set; }

        public Dictionary<string, Dictionary<string, JToken>> PrivateDataStore { get; set; }


        public JToken this[string route, string key]
        {
            get
            {
                return this.GetValueWithRouteAndKey(DataStoreType.Any, route, key);
            }
            set
            {
                this.UpdateValue(DataStoreType.Public, route, key, value);
            }
        }
    

        public JToken this[DataStoreType type, string route, string key]
        {
            get
            {
                return this.GetValueWithRouteAndKey(type, route, key);
            }
            set
            {
                this.UpdateValue(type, route, key, value);
            }
        }

        public IEnumerable<DataStoreItem> this[string key]
        {
            get
            {
                return this.GetValueAndRoutesFromDataStore(DataStoreType.Any, key);
            }

        }

        public IEnumerable<DataStoreItem> this[DataStoreType type, string key]
        {
            get
            {
                return this.GetValueAndRoutesFromDataStore(type, key);
            }
        }

        public bool RouteExists(string route, DataStoreType dataStoreType = DataStoreType.Any)
        {
            bool found = false;

            if (dataStoreType == DataStoreType.Private || dataStoreType == DataStoreType.Any)
            {
                if (PrivateDataStore.ContainsKey(route))
                {
                    found = true;
                }
            }

            if (dataStoreType == DataStoreType.Public || dataStoreType == DataStoreType.Any)
            {
                if (PrivateDataStore.ContainsKey(route))
                {
                    found = true;
                }
            }

            return found;
        }

        public bool KeyExists(string key, DataStoreType dataStoreType = DataStoreType.Any)
        {
            return this.GetValueAndRoutesFromDataStore(dataStoreType, key).Any();
        }

        public bool RouteAndKeyExists(string route, string key, DataStoreType dataStoreType = DataStoreType.Any)
        {
            var listOfKeys = this.GetValueAndRoutesFromDataStore(dataStoreType, key).ToList();
            if (listOfKeys.Any())
            {
                return listOfKeys.Any(p => p.Route.Equals(route));
            }

            return false;
        }

        private IEnumerable<DataStoreItem> GetValueAndRoutesFromDataStore(DataStoreType dataStoreType, string key)
        {
            List<DataStoreItem> valuesToReturn = new List<DataStoreItem>();

            if (dataStoreType == DataStoreType.Private || dataStoreType == DataStoreType.Any)
            {
                foreach (var keyValuePair in PrivateDataStore)
                {
                    if (keyValuePair.Value.ContainsKey(key))
                    {
                        valuesToReturn.Add(new DataStoreItem()
                        {
                          Route = keyValuePair.Key,
                          Key = key,
                          Value = keyValuePair.Value[key],
                            DataStoreType = DataStoreType.Private
                        });
                    }
                }
            }

            if (dataStoreType == DataStoreType.Public || dataStoreType == DataStoreType.Any)
            {
                foreach (var keyValuePair in PublicDataStore)
                {
                    if (keyValuePair.Value.ContainsKey(key))
                    {
                        valuesToReturn.Add(new DataStoreItem()
                        {
                            Route = keyValuePair.Key,
                            Key = key,
                            Value = keyValuePair.Value[key],
                            DataStoreType = DataStoreType.Public
                        });
                    }
                }
            }

            return valuesToReturn;
        }

        private JToken GetValueWithRouteAndKey(DataStoreType dataStoreType, string route, string key)
        {
            if (dataStoreType == DataStoreType.Private || dataStoreType == DataStoreType.Any)
            {
                if (PrivateDataStore.ContainsKey(route) && PrivateDataStore[route].ContainsKey(key))
                {
                    return PrivateDataStore[route][key];
                }
            }

            if (dataStoreType == DataStoreType.Public || dataStoreType == DataStoreType.Any)
            {
                if (PublicDataStore.ContainsKey(route) && PublicDataStore[route].ContainsKey(key))
                {
                    return PublicDataStore[route][key];
                }
            }

            return null;
        }

        private void UpdateValue(DataStoreType dataStoreType, string route, string key, JToken value)
        {
            bool found = false;

            if (dataStoreType == DataStoreType.Private || dataStoreType == DataStoreType.Any)
            {
                if (PrivateDataStore.ContainsKey(route) && PrivateDataStore[route].ContainsKey(key))
                {
                    this.PrivateDataStore[route][key] = value;
                    found = true;
                }
            }

            if (dataStoreType == DataStoreType.Public || dataStoreType == DataStoreType.Any)
            {
                if (PublicDataStore.ContainsKey(route) && PublicDataStore[route].ContainsKey(key))
                {
                    this.PrivateDataStore[route][key] = value;
                    found = true;
                }
            }

            if (!found && dataStoreType != DataStoreType.Any)
            {
                if (dataStoreType == DataStoreType.Private)
                {
                    AddModifyItemToDataStore(this.PrivateDataStore, route,key,value);
                }

                if (dataStoreType == DataStoreType.Public)
                {
                    AddModifyItemToDataStore(this.PublicDataStore, route, key, value);
                }
            }
        }

        private static void AddModifyItemToDataStore(Dictionary<string, Dictionary<string, JToken>> store,
            string route, string key, JToken value)
        {
            if (!store.ContainsKey(route))
            {
                store.Add(route, new Dictionary<string, JToken>());
            }

            if (!store[route].ContainsKey(key))
            {
                store[route].Add(key, value);
            }

            store[route][key] = value;
        }
    }
}
