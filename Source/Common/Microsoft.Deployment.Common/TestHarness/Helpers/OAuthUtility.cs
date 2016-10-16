using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Deployment.Common.TestHarness.Helpers
{    public class OAuthUtility
    {
        public static string OpenBrowserAndGetCode(string uri, string redirect, string filename, bool alwaysCreate)
        {
            if (File.Exists(filename) && !alwaysCreate)
            {
                FileInfo file = new FileInfo(filename);
                if (DateTime.Now - file.LastWriteTime < TimeSpan.FromMinutes(10))
                {
                    return File.ReadAllLines(filename)[0];
                }
            }

            var value = uri + " " + redirect + " " + filename;
            //Get new token 
            var p = Process.Start("Microsoft.Deployment.OAuth.Test.exe", value);
            p.StartInfo.RedirectStandardOutput = true;

            p.WaitForExit();

            if (File.Exists(filename))
            {
                var data = File.ReadAllLines(filename);
                return data[0];
            }

            return string.Empty;
        }
    }
}
