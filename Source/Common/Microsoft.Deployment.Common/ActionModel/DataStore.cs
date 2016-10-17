using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.ActionModel
{
    public class DataStore
    {
        public string CurrentRoutePage { get; set; }
        public string DeploymentIndex { get; set; }

        public string CurrentRoute => CurrentRoutePage + "-" + DeploymentIndex;

        public Dictionary<string,Dictionary<string,JToken>> PublicDataStore { get; set; }

        public Dictionary<string, Dictionary<string, JToken>> PrivateDataStore { get; set; }

        public IEnumerable<JToken> GetValueFromDataStore(DataStoreType dataStoreType, string key)
        {
            List<JToken> valuesToReturn = new List<JToken>();

            if (dataStoreType == DataStoreType.Private || dataStoreType == DataStoreType.Any)
            {
                foreach (var keyValuePair in PrivateDataStore)
                {
                    if (keyValuePair.Value.ContainsKey(key))
                    {
                        valuesToReturn.Add(keyValuePair.Value[key]);
                    }
                }
            }

            if (dataStoreType == DataStoreType.Public || dataStoreType == DataStoreType.Any)
            {
                foreach (var keyValuePair in PublicDataStore)
                {
                    if (keyValuePair.Value.ContainsKey(key))
                    {
                        valuesToReturn.Add(keyValuePair.Value[key]);
                    }
                }
            }

            return valuesToReturn;
        }

        public JToken GetValueWithRouteAndKey(DataStoreType dataStoreType, string route, string key)
        {
            if (dataStoreType == DataStoreType.Private || dataStoreType == DataStoreType.Any)
            {
                return PrivateDataStore[route][key];
            }

            if (dataStoreType == DataStoreType.Public || dataStoreType == DataStoreType.Any)
            {
                return PublicDataStore[route][key];
            }

            throw new Exception("Must specify type");
        }
    }
}
