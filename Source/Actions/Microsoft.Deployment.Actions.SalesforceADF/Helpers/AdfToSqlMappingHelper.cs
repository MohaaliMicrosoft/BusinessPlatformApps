using System.Collections.Generic;

namespace Microsoft.Deployment.Actions.SalesforceADF.Helpers
{
    public static class AdfToSqlMappingHelper
    {
        public static Dictionary<string, string> TypeMappings = new Dictionary<string, string>
        {
            {"id","nvarchar" },
            {"string", "nvarchar"},
            {"boolean", "tinyint" },
            {"double", "float" },
            {"date", "datetime" },
            {"dateTime", "datetime" },
            {"tnsaddress", "nvarchar" },
         };
    }
}
