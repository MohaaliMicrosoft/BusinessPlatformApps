using System;
using System.Collections.Generic;
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

            throw new Exception("Canot find key with route");
        }

        private void UpdateValue(DataStoreType dataStoreType, string route, string key, JToken value)
        {
            if (dataStoreType == DataStoreType.Private || dataStoreType == DataStoreType.Any)
            {
                if (PrivateDataStore.ContainsKey(route) && PrivateDataStore[route].ContainsKey(key))
                {
                    PrivateDataStore[route][key] = value;
                }
            }

            if (dataStoreType == DataStoreType.Public || dataStoreType == DataStoreType.Any)
            {
                if (PublicDataStore.ContainsKey(route) && PublicDataStore[route].ContainsKey(key))
                {
                    PrivateDataStore[route][key] = value;
                }
            }
        }

        public JToken this[string route, string key]
        {
            get
            {
                return this.GetValueWithRouteAndKey(DataStoreType.Any, route, key);
            }
            set
            {
                this.UpdateValue(DataStoreType.Any, route, key, value);
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
                this.UpdateValue(DataStoreType.Any, route, key, value);
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
    }
}
