using DSM.Controller.Shared.Models;
using DSM.Controller.Utils;
using DSM.Core.Ops;
using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSM.Controller.AppServices.IIS
{
    public class SiteController : DSMController
    {
        public SiteController(string authToken) : base(authToken)
        {
            _ = Sites;
        }
        private IEnumerable<Core.Models.Site> _sites = null;
        private readonly ServerManager serverManager = new ServerManager();

        public IEnumerable<Core.Models.Site> Sites
        {
            get
            {
                if (_sites == null)
                {

                    SiteCollection sites = serverManager.Sites;
                    List<Core.Models.Site> siteList = new List<Core.Models.Site>();

                    IEnumerable<IEnumerable<Core.Models.Site>> collection = sites.Select(site =>
                    site.Applications.Select(app =>
                     new Core.Models.Site
                     {
                         ApplicationPoolName = app.ApplicationPoolName,
                         EnabledProtocols = app.EnabledProtocols,
                         PhysicalPath = app.VirtualDirectories.First().PhysicalPath.ExplicitPath(),
                         LogFileDirectory = site.LogFile.Directory.ExplicitPath(),
                         LogFileEnabled = site.LogFile.Enabled,
                         IISSiteId = site.Id,
                         LogFormat = site.LogFile.LogFormat.ToString(),
                         LogPeriod = site.LogFile.Period.ToString(),
                         MachineName = Environment.MachineName,
                         MaxBandwitdh = site.Limits.MaxBandwidth,
                         MaxConnections = site.Limits.MaxConnections,
                         Name = site.Name.Concatenate(app.Path),
                         ServerAutoStart = site.ServerAutoStart,
                         State = "",//site.State.GetString(),
                         TraceFailedRequestsLoggingDirectory = site.TraceFailedRequestsLogging.Directory,
                         TraceFailedRequestsLoggingEnabled = site.TraceFailedRequestsLogging.Enabled,
                         LastUpdated = DateTime.Now,
                         WebConfigBackupDirectory = "",
                         LastCheckTime = DateTime.Now,
                         SendAlertMailWhenUnavailable = true,
                         NetFrameworkVersion = "",
                         IsAvailable = true,
                         DateDeleted = new DateTime(1900, 01, 01),
                         WebConfigLastBackupDate = new DateTime(1900, 01, 01),
                         AppType = app.Path == "/" ? "Root" : "Virtual App",
                         RawBindings = app.Path == "/" ? site.Bindings : null
                     })); //NetFrameworkVersion

                    collection.ToList().ForEach(x => siteList.AddRange(x));

                    _sites = siteList.AsEnumerable();
                    return siteList.AsEnumerable();
                }
                else
                {
                    return _sites;
                }
            }
        }
        public void Clear()
        {
            _sites = null;
        }
        public void PostSites()
        {

            IEnumerable<Core.Models.Site> resultSet = WebTransfer.PostSite(Sites)?.ToList();
            logManager.Write("Post Method works fine.");

            IEnumerable<Core.Models.Site> query = _sites.Join(resultSet, target => target.IISSiteId, source => source.IISSiteId,
                (source, target) => new Core.Models.Site
                {
                    Id = target.Id,
                    ApplicationPoolName = source.ApplicationPoolName,
                    AppType = source.AppType,
                    DateDeleted = source.DateDeleted,
                    EnabledProtocols = source.EnabledProtocols,
                    IISSiteId = source.IISSiteId,
                    IsAvailable = source.IsAvailable,
                    LastCheckTime = source.LastCheckTime,
                    LastUpdated = source.LastUpdated,
                    LogFileDirectory = source.LogFileDirectory,
                    LogFileEnabled = source.LogFileEnabled,
                    LogFormat = source.LogFormat,
                    LogPeriod = source.LogPeriod,
                    MachineName = source.MachineName,
                    MaxBandwitdh = source.MaxBandwitdh,
                    MaxConnections = source.MaxConnections,
                    Name = source.Name,
                    NetFrameworkVersion = source.NetFrameworkVersion,
                    PhysicalPath = source.PhysicalPath,
                    RawBindings = source.RawBindings,
                    SendAlertMailWhenUnavailable = source.SendAlertMailWhenUnavailable,
                    ServerAutoStart = source.ServerAutoStart,
                    State = source.State,
                    TraceFailedRequestsLoggingDirectory = source.TraceFailedRequestsLoggingDirectory,
                    TraceFailedRequestsLoggingEnabled = source.TraceFailedRequestsLoggingEnabled,
                    WebConfigBackupDirectory = source.WebConfigBackupDirectory,
                    WebConfigLastBackupDate = source.WebConfigLastBackupDate
                });

            _sites = query;
        }

    }
}
