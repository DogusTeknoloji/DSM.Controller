using DSM.Core.Interfaces.AppServices;
using DSM.Core.Ops;
using System.IO;
using System.Linq;

namespace DSM.Controller.Utils
{
    public static class Extensions
    {
        public static string _authToken = null;

        public static string ExplicitPath(this string path)
        {
            if (path == null)
            {
                return null;
            }

            return path.Replace("%SystemDrive%\\", FileOperations.GetSystemDrive());
        }

        public static string Concatenate(this string value, params string[] parameters)
        {
            string prms = string.Join("", parameters);
            string result = string.Join(string.Empty, value, prms);
            return result;
        }

        public static string GetWebConfigContent(this ISite site)
        {
            string sitePhysicalPath = site.PhysicalPath;

            if (!Directory.Exists(sitePhysicalPath))
            {
                return null;
            }

            string[] files = Directory.GetFiles(sitePhysicalPath, "web.config", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                string content = File.ReadAllText(files.First());
                return content;
            }
            return null;
        }


    }
}
