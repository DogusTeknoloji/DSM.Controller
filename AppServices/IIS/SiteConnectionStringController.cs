using DSM.Controller.Shared.Interfaces;
using DSM.Controller.Shared.Models;
using DSM.Core.Interfaces.AppServices;
using DSM.Core.Models;
using DSM.Core.Ops;
using DSM.Core.PackageManager;
using System.Collections;
using System.Collections.Generic;

namespace DSM.Controller.AppServices.IIS
{
    public class SiteConnectionStringController : DSMController,
                                                  IDSMSiteAssignable<ISiteConnectionString>
    {
        public SiteConnectionStringController(string authToken) : base(authToken)
        {
        }

        public IEnumerable<ISiteConnectionString> Get(ISite site)
        {
            return WebTransfer.GetSiteConnectionStrings(site.Id);
        }

        public void Post(ISite site)
        {
            ISiteWebConfiguration webconfig = site.GetConfiguration();
            if (webconfig == null)
            {
                return;
            }

            IEnumerable<SiteConnectionString> connectionStrings = site.GetRawConnectionStrings();

            if (connectionStrings == null)
            {
                return;
            }
            WebTransfer.PostSiteConnectionString(connectionStrings);
        }

        public void Post(IEnumerable<ISite> sites)
        {
            List<SiteConnectionString> siteConnectionStrings = new List<SiteConnectionString>();
            foreach (ISite site in sites)
            {
                if (site.AppType != "Root")
                {
                    continue;
                }

                ISiteWebConfiguration webconfig = site.GetConfiguration();
                if (webconfig == null)
                {
                    continue;
                }

                IEnumerable<SiteConnectionString> connectionStrings = site.GetRawConnectionStrings();

                if (connectionStrings != null)
                {
                    siteConnectionStrings.AddRange(connectionStrings);
                }
            }

            if (siteConnectionStrings.Count > 0)
            {
                PackageManager packageManager = PackageManager.GetManager();
                IEnumerable packs = packageManager.GetPacks(siteConnectionStrings);
                foreach (IEnumerable<SiteConnectionString> pack in packs)
                {
                    WebTransfer.PostSiteConnectionString(pack);
                }
            }
        }



    }
}