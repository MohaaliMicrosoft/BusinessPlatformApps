using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Deployment.Common.Tags;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.AppLoad
{
    public class AppFactory
    {
        private CompositionContainer container;

        private Dictionary<string, UIPage> allPages = new Dictionary<string, UIPage>();

        [ImportMany]
        public IEnumerable<ITagHandler> AllTagHandlers;

        [ImportMany]
        public IEnumerable<IAction> AllActions;

        [ImportMany]
        public IEnumerable<IActionExceptionHandler> ActionExceptionsHandlers;

        [ImportMany]
        public IEnumerable<IActionRequestInterceptor> RequestInterceptors;


        public Dictionary<string, ITagHandler> TagHandlers { get; } = new Dictionary<string, ITagHandler>();
        public Dictionary<string, IAction> Actions { get; } = new Dictionary<string, IAction>();
        public Dictionary<string, App> Apps { get; } = new Dictionary<string, App>();

        public string SiteCommonPath { get; set; }
        public string AppPath { get; private set; }

        public AppFactory()
        {
            this.SetAppsPath();

            this.LoadBinaries();
            this.LoadAllPages();

            this.InstantiateAllApps();

        }

        private void InstantiateAllApps()
        {
            var templateFiles = Directory.GetFiles(this.AppPath, Constants.InitFile, SearchOption.AllDirectories);
            var parentDirectories = templateFiles.Select(Directory.GetParent);
            foreach (var dir in parentDirectories)
            {
                string initFilePath = Path.Combine(dir.FullName, Constants.InitFile);
                string file = File.ReadAllText(initFilePath);

                App app = new App
                {
                    Name = dir.Name,
                    AppFilePath = dir.FullName,
                    AppRelativeFilePath = dir.FullName.Substring(dir.FullName.IndexOf("\\" + Constants.AppsPath + "\\", StringComparison.Ordinal))
                };

                this.Apps.Add(dir.Name, app);
                this.Parse(file, app);
            }
        }

        private void LoadBinaries()
        {
            var catalog = new AggregateCatalog();
            string path = System.AppDomain.CurrentDomain.BaseDirectory;
            catalog.Catalogs.Add(new DirectoryCatalog(path));
            if (Directory.Exists(path + "bin"))
            {
                catalog.Catalogs.Add(new DirectoryCatalog(path + "bin"));
            }

            container = new CompositionContainer(catalog);
            try
            {
                this.container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }
        }

        private void LoadAllPages()
        {
            var templateFiles = Directory.GetFiles(this.AppPath, Constants.InitFile, SearchOption.AllDirectories);
            var parentDirectories = templateFiles.Select(Directory.GetParent);
            foreach (var dir in parentDirectories)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, Constants.AppsWebPath)))
                {
                    string[] htmlPages = Directory.GetFiles(Path.Combine(dir.FullName, Constants.AppsWebPath), "*.html", SearchOption.AllDirectories);
                    string relativeUrl = Constants.AppsPath + Path.Combine(dir.FullName.Replace(this.AppPath, ""), Constants.AppsWebPath);
                    IEnumerable<string> relativeFilePaths = htmlPages.Select(p => p.Substring(p.IndexOf("\\" + Constants.AppsPath + "\\", StringComparison.Ordinal)));
                    this.AddPages(dir.Name, relativeFilePaths);
                }
            }

            string[] htmlPagesCommon = Directory.GetFiles(Path.Combine(this.SiteCommonPath, Constants.AppsWebPath), "*.html", SearchOption.AllDirectories);
            string relativeUrlCommon = Path.Combine(Constants.SiteCommonPath, Constants.AppsWebPath);
            IEnumerable<string> relativeFilePathsCommon = htmlPagesCommon.Select(p => p.Substring(p.IndexOf("\\" + Constants.SiteCommonPath + "\\", StringComparison.Ordinal)));
            this.AddPages("$SiteCommon$", relativeFilePathsCommon);
        }

        private void AddPages(string appName, IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                string userGeneratedPath = file.Substring(file.IndexOf($"\\{Constants.AppsWebPath}\\", StringComparison.Ordinal) + 5);
                userGeneratedPath = Path.Combine(appName, userGeneratedPath);

                this.allPages.Add(userGeneratedPath,
                    new UIPage()
                    {
                        PageName = file.Split('\\').Last(),
                        AppName = appName,
                        RoutePageName = file.Split('\\').Last().Replace(".html", ""),
                        Path = file.Replace(".html",""),
                        UserGeneratedPath = userGeneratedPath
                    });
            }
        }


        private void SetAppsPath()
        {
            string path = System.AppDomain.CurrentDomain.BaseDirectory;
            string appsPathWithBin = Path.Combine(path, Constants.BinPath, Constants.AppsPath);
            string appsPathWithoutBin = Path.Combine(path, Constants.AppsPath);

            // Find the apps folder
            if (Directory.Exists(appsPathWithoutBin))
            {
                this.SiteCommonPath = Path.Combine(path, Constants.SiteCommonPath);
                this.AppPath = appsPathWithoutBin;
            }
            else if (Directory.Exists(appsPathWithBin))
            {
                this.SiteCommonPath = Path.Combine(path, Constants.BinPath, Constants.SiteCommonPath);
                this.AppPath = appsPathWithBin;
            }

            if (string.IsNullOrEmpty(this.AppPath) || string.IsNullOrEmpty(this.SiteCommonPath))
            {
                throw new Exception("No Apps found or SiteCommon path not found");
            }
        }


        private void Parse(string file, App app)
        {
            JObject obj = JsonUtility.GetJsonObjectFromJsonString(file);

            List<string> rootTags = new List<string>();

            foreach (var child in obj.Root.Children())
            {
                rootTags.Add(child.Path);
            }

            foreach (var child in obj.Root.Children())
            {
                //this.ParseTag(child, obj, app);

                if (rootTags.Contains(child.Path))
                {
                    TagHandlerUtility.ParseTag(child, obj, app, this.allPages, this.Actions, this.AllTagHandlers.Where(t => t.Tag == child.Path));
                }

                TagHandlerUtility.ParseTag(child, obj, app, this.allPages, this.Actions, this.AllTagHandlers);
            }
        }

        private void ParseTag(JToken obj, JObject root, App app)
        {
            foreach (var tag in this.AllTagHandlers)
            {
                if (tag.Tag.Equals(obj.Path.Split('.').Last(), StringComparison.OrdinalIgnoreCase))
                {
                    if (obj.Children().First().Type == JTokenType.Array)
                    {
                        tag.ProcessTag(obj.Children().First(), root, this.allPages, this.Actions, app);
                    }
                    else
                    {
                        tag.ProcessTag(obj.Value<JToken>(), root, this.allPages, this.Actions, app);
                    }
                }
            }

            if (obj.HasValues && obj.Children().First().Children().Any())
            {
                foreach (var child in obj.Children().First().Children())
                {
                    this.ParseTag(child, root, app);
                }
            }
        }
    }
}