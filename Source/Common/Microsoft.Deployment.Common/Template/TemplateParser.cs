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

namespace Microsoft.Deployment.Common.Template
{
    public class TemplateParser
    {
        private CompositionContainer container;

        // Import from DLL Folders
        [ImportMany]
        public IEnumerable<ITagHandler> AllTagHandlers;

        [ImportMany]
        public IEnumerable<IAction> AllActions;

        [ImportMany]
        public IEnumerable<IActionExceptionHandler> AllActionExceptionHandlers;

        [ImportMany]
        public IEnumerable<IActionRequestInterceptor> AllRequestInterceptors;

        private List<UIPage> allPages;

        public string TemplatePath { get; private set; }

        public List<Template> Templates { get; private set; }

        public TemplateParser()
        {
            this.Templates = new List<Template>();
            this.allPages = new List<UIPage>();
            this.Load();
            this.LoadAllPages();
            this.GetAllTemplates();
            this.TemplatePath = GetTemplatePath();
        }

        private void Load()
        {
            var catalog = new AggregateCatalog();
            string path = System.AppDomain.CurrentDomain.BaseDirectory;
            catalog.Catalogs.Add(new DirectoryCatalog(path));
            //if (Directory.Exists(path + "bin"))
            //{
            //    catalog.Catalogs.Add(new DirectoryCatalog(path + "bin"));
            //}

            //string templatePath = GetTemplatePath();

            //string[] dir = System.IO.Directory.GetDirectories(templatePath);

            //foreach (var d in dir)
            //{
            //    System.IO.DirectoryInfo directoryInfo = new DirectoryInfo(d);
            //    string customActionsPath = d + Constants.ActionsPath;

            //    if (Directory.Exists(customActionsPath))
            //    {
            //        catalog.Catalogs.Add(new DirectoryCatalog(customActionsPath));
            //    }
            //}

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
            string templatePath = GetTemplatePath();

            string[] dir = Directory.GetDirectories(templatePath);

            foreach (var d in dir)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(d);

                if (!directoryInfo.Name.EqualsIgnoreCase("common"))
                {
                    string customUiPath = d + Constants.TemplateWebPath;
                    Directory.GetFiles(customUiPath, "*.html").ToList().ForEach(p => this.AddUIPages(p, directoryInfo));
                }
            }
        }

        private void AddUIPages(string page, DirectoryInfo commonUiDirectoryInfo)
        {
            var fileInfo = new System.IO.FileInfo(page);
            var pageShortened = page.Substring(page.IndexOf('/'));
            string relativePath = pageShortened.Substring(pageShortened.IndexOf("/Template"));

            relativePath = relativePath.Replace("//", "/");
            relativePath = relativePath.Replace("\\\\", "/");
            relativePath = relativePath.Replace("\\", "/");
            relativePath = relativePath.Replace(fileInfo.Extension, "");

            UIPage uiPage = new UIPage
            {
                TemplateName = commonUiDirectoryInfo.Name,
                PageName = fileInfo.Name,
                Path = relativePath,
                RoutePageName = fileInfo.Name.Replace(" ","").Replace(fileInfo.Extension,"")
            };

            this.allPages.Add(uiPage);
        }

        private void GetAllTemplates()
        {
           
            string templatePath = GetTemplatePath();

            string[] dir = System.IO.Directory.GetDirectories(templatePath);

            foreach (var d in dir)
            {
                System.IO.DirectoryInfo directoryInfo = new DirectoryInfo(d);

                if (!directoryInfo.Name.EqualsIgnoreCase("common"))
                {
                    string initFilePath = d + Constants.TemplateInitFile;
                    string file = System.IO.File.ReadAllText(initFilePath);
                    Template template = new Template { TemplateName = directoryInfo.Name };
                    this.Templates.Add(template);
                    this.Parse(file, template);
                }
            }
        }

        private string GetTemplatePath()
        {
            string folderPath = System.AppDomain.CurrentDomain.BaseDirectory;
            string templatePath = folderPath + Constants.TemplatePath;
            if (!Directory.Exists(templatePath))
            {
                templatePath = folderPath + Constants.TemplatePathMsi;
                if (!Directory.Exists(templatePath))
                {
                    throw new Exception("No Templates Found");
                }
            }
            return templatePath;
        }

        private void Parse(string file, Template template)
        {
            JObject obj = JsonUtility.GetJsonObjectFromJsonString(file);
            ;
            foreach (var child in obj.Root.Children())
            {
                this.ParseTag(child, obj, template);
            }
        }

        private void ParseTag(JToken obj, JObject root, Template template)
        {
            foreach (var tag in this.AllTagHandlers)
            {
                if (tag.Tag.Equals(obj.Path.Split('.').Last(), StringComparison.OrdinalIgnoreCase))
                {
                    if (obj.Children().First().Type == JTokenType.Array)
                    {
                        tag.ProcessTag(obj.Children().First(), root, this.allPages, this.AllActions, template);
                    }
                    else
                    {
                        tag.ProcessTag(obj.Value<JToken>(), root, this.allPages, this.AllActions, template);
                    }
                }
            }

            if (obj.HasValues && obj.Children().First().Children().Any())
            {
                foreach (var child in obj.Children().First().Children())
                {
                    this.ParseTag(child, root, template);
                }
            }
        }
    }
}