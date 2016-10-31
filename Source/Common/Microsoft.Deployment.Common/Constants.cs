﻿namespace Microsoft.Deployment.Common
{
    public static class Constants
    {
        public const string AppsPath = "Apps";
        public const string AppsWebPath = "Web";
        public const string SiteCommonPath = "SiteCommon";

        public const string ActionsPath = "Actions";
        public const string InitFile = "init.json";
        public const string BinPath = "bin";

        public const string AzureManagementApi = "https://management.azure.com/";
        public const string AzureManagementCoreApi = "https://management.core.windows.net/";
        public const string AzureWebSite = ".scm.azurewebsites.net/";

        public const string AzureAuthUri = "https://login.microsoftonline.com/{0}/oauth2/authorize?";
        public const string AzureTokenUri = "https://login.microsoftonline.com/{0}/oauth2/token";

        public const string MicrosoftClientId = "6b317a7c-0749-49bd-9e8c-d906aa43f64b";
        public const string MicrosoftClientSecret = "";
        public const string WebsiteRedirectPath = "/redirect.html";
        public const string AppInsightsKey = "74bc59f2-6526-41b1-ab84-370532ec5d42";
    }
}