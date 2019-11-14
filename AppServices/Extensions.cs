using DSM.Controller.AppServices.IIS.Models;
using DSM.Controller.Utils;
using DSM.Core.Interfaces.AppServices;
using DSM.Core.Models;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace DSM.Controller.AppServices
{
    public static class Extensions
    {

        public static string GetString(this ObjectState state)
        {
            try
            {
                return state.ToString();
            }
            catch (Exception)
            {

            }
            return null;
        }

        public static SiteState Convert(this ObjectState state)
        {
            try
            {
                return (SiteState)state;
            }
            catch (Exception)
            {
                return SiteState.NotMapped;
            }
        }

        public static SiteState Swap(this string state)
        {
            SiteState siteState = (SiteState)Enum.Parse(typeof(SiteState), state);
            return siteState;
        }

        public static void BindDBId(this IEnumerable<dynamic> results, ref IEnumerable<ISite> sites)
        {
            if (results == null)
            {
                return;
            }

            sites.ToList().ForEach(x => x.Id = results.FirstOrDefault(y => y.MachineName == x.MachineName && y.Name == x.Name).Id);
        }

        public static IEnumerable<SitePackage> Packages(this ISite site)
        {
            string configContent = site.GetWebConfigContent();

            if (configContent == null)
            {
                return null;
            }

            IList<SitePackage> packages = new List<SitePackage>();

            string frameworkVersion = configContent.NetFrameworkVersionFromXml();
            if (frameworkVersion != null)
            {
                packages.Add(new SitePackage { Name = "NetFrameworkVersion", NewVersion = frameworkVersion, SiteId = site.Id });
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(configContent);

            XmlNamespaceManager nsm = new XmlNamespaceManager(doc.NameTable);
            nsm.AddNamespace("ns", "urn:schemas-microsoft-com:asm.v1");
            XmlNodeList nodes = null;
            nodes = doc.SelectNodes("//configuration/runtime/ns:assemblyBinding/ns:dependentAssembly", nsm);
            if (nodes.Count < 1)
            {
                return packages;
            }

            XmlNodeList name = nodes[0].SelectNodes("//ns:assemblyIdentity/@name", nsm);
            string[] names = new string[name.Count]; // 11 
            for (int j = 0; j < name.Count; j++) //  11 j -0
            {
                names[j] = name[j].InnerText;
            }

            XmlNodeList version = nodes[0].SelectNodes("//ns:bindingRedirect/@newVersion", nsm); // 11
            for (int j = 0; j < version.Count; j++)
            {
                packages.Add(new SitePackage { Name = names[j], NewVersion = version[j].InnerText, SiteId = site.Id });
            }
            return packages;

        }

        public static IEnumerable<SiteBinding> Bindings(this ISite site)
        {
            BindingCollection collection = (BindingCollection)site.RawBindings;
            string consoleText = string.Format("Collection:{0}", collection);

            return collection?.Select(x => new SiteBinding
            {
                Host = x.Host,
                //BindingInformation = x.BindingInformation,
                IpAddress = x.EndPoint?.Address.ToString(),
                IpAddressFamily = x.EndPoint?.AddressFamily.ToString(),
                Port = x.EndPoint?.Port.ToString(),
                IsSSLBound = x.CertificateHash != null,
                Protocol = x.Protocol,
                SiteId = site.Id
            });
        }

        public static string NetFrameworkVersionFromXml(this string xmlContent)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlContent);

            XmlNodeList nodes = xmlDocument.SelectNodes("//system.web/compilation/@targetFramework");
            foreach (XmlNode node in nodes)
            {
                return node.InnerText;
            }
            return null;
        }


    }
}
