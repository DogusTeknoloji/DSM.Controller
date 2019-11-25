using DSM.Controller.Tracker.ConnectionString;
using DSM.Core.Interfaces.AppServices;
using DSM.Core.Models;
using DSM.Core.Ops;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;

namespace DSM.Controller.AppServices
{
    public static class SiteConnectionstringExtensions
    {
        /// <summary>
        /// Get Raw Connectionstrings from site 
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static IEnumerable<SiteConnectionString> GetRawConnectionStrings(this ISite site)
        {
            try
            {
                ISiteWebConfiguration webconfig = site.GetConfiguration();
                if (webconfig == null)
                {
                    return null;
                }

                IEnumerable<KeyValuePair<string, string>> rawConnectionstrings = webconfig.ContentRaw.GetRawConnectionStrings();
                IEnumerable<SiteConnectionString> connectionStrings = rawConnectionstrings?.Select(cs => GetConnectionString(cs.Value, cs.Key, site.Id));

                return connectionStrings;
            }
            catch (Exception ex)
            {
                ExceptionHandler.Exception(ex);
                return null;
            }
        }
        /// <summary>
        /// Get Raw Connectionstrings from site
        /// </summary>
        /// <param name="contentRaw"></param>
        /// <returns></returns>
        private static IEnumerable<KeyValuePair<string, string>> GetRawConnectionStrings(this string contentRaw)
        {
            try
            {
                List<KeyValuePair<string, string>> outputs = new List<KeyValuePair<string, string>>();
                XmlDocument document = new XmlDocument();
                document.LoadXml(contentRaw);
                XmlNodeList nodes = document.SelectNodes("//connectionStrings/add");
                foreach (XmlNode xmlNode in nodes)
                {
                    string addr = xmlNode.Attributes["connectionString"].InnerText;
                    string name = xmlNode.Attributes["name"].InnerText;
                    outputs.Add(new KeyValuePair<string, string>(name, addr));
                }

                document.LoadXml(contentRaw);
                nodes = document.SelectNodes("appSettings/add");
                if (nodes.Count < 1)
                {
                    return outputs;
                }

                foreach (XmlNode xmlNode1 in nodes)
                {
                    string addr = xmlNode1.Attributes["value"].InnerText;
                    if (addr.Contains("Data Source"))
                    {
                        string name = xmlNode1.Attributes["key"].InnerText;
                        outputs.Add(new KeyValuePair<string, string>(name, addr));
                    }
                }

                return outputs;
            }
            catch (Exception ex)
            {
                ExceptionHandler.Exception(ex);
                return null;
            }
        }
        /// <summary>
        /// Gathers Connectionstring object from raw text -> connection string 
        /// </summary>
        /// <param name="connectionString">Raw String</param>
        /// <param name="connectionName">Name of the Connection</param>
        /// <param name="siteId">Foreign Key of between Site and Connectionstring object</param>
        /// <returns></returns>
        private static SiteConnectionString GetConnectionString(string connectionString, string connectionName, long siteId)
        {

            if (connectionString.Contains(("metadata=res://")))
            {
                try
                {
                    XConsole.WriteLine("EF BASED CONNECTION STRING DETECTED");
                    EFConnectionStringBuilder entityConnectionString = new EFConnectionStringBuilder(connectionString);
                    connectionString = entityConnectionString.ProviderConnectionString;
                    XConsole.WriteLine(connectionString);
                }
                catch (Exception ex)
                {
                    ExceptionHandler.Exception(ex);
                }
            }

            try
            {
                SqlConnectionStringBuilder sqlConnectionString = new SqlConnectionStringBuilder(connectionString);
                string serverName = string.Empty;
                int portInfo = 1433;
                string[] spDSource = sqlConnectionString.DataSource.Split(',');
                if (spDSource.Length > 1)
                {
                    serverName = spDSource[0];
                    int.TryParse(spDSource[1], out portInfo);
                }
                else
                {
                    serverName = sqlConnectionString.DataSource;
                }

                return new SiteConnectionString
                {
                    SiteId = siteId,
                    RawConnectionString = connectionString,
                    ServerName = serverName,
                    Port = portInfo,
                    DatabaseName = sqlConnectionString.InitialCatalog,
                    UserName = sqlConnectionString.UserID,
                    Password = sqlConnectionString.Password,
                    IsAvailable = true,
                    LastCheckTime = DateTime.Now,
                    ConnectionName = connectionName
                };
            }
            catch (Exception ex)
            {
                ExceptionHandler.Exception(ex);
                return null;
            }
        }
    }
}
