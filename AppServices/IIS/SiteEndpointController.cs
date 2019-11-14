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
    public class SiteEndpointController : DSMController,
                                         IDSMSiteAssignable<ISiteEndpoint>
    {
        public SiteEndpointController(string authToken) : base(authToken)
        {
        }

        public IEnumerable<ISiteEndpoint> Get(ISite site)
        {
            return WebTransfer.GetSiteEndpoints(site.Id);
        }

        public void Post(ISite site)
        {
            IEnumerable<SiteEndpoint> endpoints = site.GetRawEndpoints();

            if (endpoints == null)
            {
                return;
            }

            WebTransfer.PostSiteEndpoint(endpoints);
        }

        public void Post(IEnumerable<ISite> sites)
        {
            List<SiteEndpoint> siteEndpoints = new List<SiteEndpoint>();
            foreach (ISite site in sites)
            {
                if (site.AppType != "Root")
                {
                    continue;
                }

                IEnumerable<SiteEndpoint> endpoints = site.GetRawEndpoints();

                if (endpoints != null)
                {
                    siteEndpoints.AddRange(endpoints);
                }
            }
            if (siteEndpoints.Count > 0)
            {
                PackageManager packageManager = PackageManager.GetManager();
                IEnumerable packs = packageManager.GetPacks(siteEndpoints);
                foreach (IEnumerable<SiteEndpoint> pack in packs)
                {
                    WebTransfer.PostSiteEndpoint(pack);
                }
            }
        }
    }
}
