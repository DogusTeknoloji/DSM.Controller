using DSM.Core.Interfaces.AppServices;
using DSM.Core.Models;
using DSM.Core.Ops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace DSM.Controller.AppServices
{
    public static class SiteEndpointExtensions
    {
        public static IEnumerable<SiteEndpoint> GetRawEndpoints(this ISite site)
        {
            try
            {
                ISiteWebConfiguration webconfig = site.GetConfiguration();
                if (webconfig == null)
                {
                    return null;
                }

                IEnumerable<KeyValuePair<string, string>> rawEndpoints = webconfig.ContentRaw.GetRawEndpoints(site.Name);
                IEnumerable<SiteEndpoint> endpoints = rawEndpoints?.Select(ep => new SiteEndpoint
                {
                    SiteId = site.Id,
                    Site = (Site)site,
                    EndpointName = ep.Key,
                    EndpointUrl = ep.Value,
                    Port = ep.Value.GetPort(),
                    IsAvailable = true,
                    LastCheckDate = DateTime.Now,
                    DeleteStatus = false,
                    SendAlertMailWhenUnavailable = true
                });
                return endpoints;
            }
            catch (Exception ex)
            {
                ExceptionHandler.Exception(ex);
                return null;
            }
        }
        /// <summary>
        /// Get port number from endpoint
        /// </summary>
        /// <param name="rawEndpoint"></param>
        /// <returns></returns>
        private static int GetPort(this string rawEndpoint)
        {
            try
            {
                char splitter = ':';
                char endOfUrl = '/';
                int endpointPort = 80;

                string excludedString = rawEndpoint.ExcludeHttpTags();
                if (excludedString.Contains(splitter))
                {
                    string portSplitVal = excludedString.Split(splitter).Last();
                    int portSplitIndex = portSplitVal.IndexOf(endOfUrl);
                    string port = portSplitVal.Substring(0, portSplitIndex);
                    int.TryParse(port, out endpointPort);
                }
                return endpointPort;
            }
            catch (Exception ex)
            {
                ExceptionHandler.Exception(ex);
                return 80;
            }
        }
        /// <summary>
        /// Get Raw endpoints from site
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private static IEnumerable<KeyValuePair<string, string>> GetRawEndpoints(this string xmlContent, string siteName = "")
        {
            try
            {
                List<KeyValuePair<string, string>> outputs = new List<KeyValuePair<string, string>>();
                XmlDocument document = new XmlDocument();
                document.LoadXml(xmlContent);
                XmlNodeList nodes = document.SelectNodes("//client/endpoint");
                foreach (XmlNode xmlNode in nodes)
                {
                    string addr = xmlNode.Attributes["address"].InnerText;
                    string name = xmlNode.Attributes["name"].InnerText;
                    outputs.Add(new KeyValuePair<string, string>(name, addr));
                }

                document.LoadXml(xmlContent);
                nodes = document.SelectNodes("//appSettings/add");
                if (nodes.Count < 1)
                {
                    return outputs;
                }

                foreach (XmlNode xmlNode1 in nodes)
                {
                    string addr = xmlNode1.Attributes["value"].InnerText;
                    if (addr.Contains("http"))
                    {
                        string name = xmlNode1.Attributes["key"].InnerText;
                        outputs.Add(new KeyValuePair<string, string>(name, addr));
                    }
                }
                return outputs;
            }
            catch (XmlException ex)
            {
                ExceptionHandler.XmlException(ex);

                XConsole.WriteLine(ex.ToString());
                XConsole.WriteLine(siteName, Core.Ops.ConsoleTheming.ConsoleColorSetGreen.Instance);
                XConsole.WriteLine(xmlContent.Length.ToString(), Core.Ops.ConsoleTheming.ConsoleColorSetGreen.Instance);
                XConsole.WriteLine(xmlContent, Core.Ops.ConsoleTheming.ConsoleColorSetRed.Instance);
                return null;
            }
        }
        /// <summary>
        /// Exclude http/https tags from url
        /// </summary>
        /// <param name="rawEndpoint"></param>
        /// <returns></returns>
        private static string ExcludeHttpTags(this string rawEndpoint)
        {
            string excludedString = rawEndpoint.Replace("http:", "");
            excludedString = excludedString.Replace("https", "");
            return excludedString;
        }
    }
}
