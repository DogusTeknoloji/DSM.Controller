using DSM.Controller.Shared.Interfaces;
using DSM.Controller.Shared.Models;
using DSM.Controller.Utils;
using DSM.Core.Interfaces.AppServices;
using DSM.Core.Models;
using DSM.Core.Ops;
using DSM.Core.PackageManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DSM.Controller.AppServices.IIS
{
    public class WebConfigController : DSMController,
                                      IDSMSiteAssignable<SiteWebConfiguration>
    {
        public WebConfigController(string authToken) : base(authToken)
        {
        }

        public IEnumerable<SiteWebConfiguration> Get(ISite site)
        {
            string configContent = site.GetWebConfigContent();

            if (configContent == null)
            {
                return null;
            }

            return new[]
            {
                new SiteWebConfiguration { Id = site.Id, ContentRaw = configContent }
            };
        }

        public void Post(ISite site)
        {
            IEnumerable<SiteWebConfiguration> webConfiguration = Get(site);

            if (webConfiguration != null)
            {
                WebTransfer.PostSiteWebConfiguration(webConfiguration);
            }
        }

        public void Post(IEnumerable<ISite> sites)
        {
            List<SiteWebConfiguration> siteWebConfigurations = new List<SiteWebConfiguration>();
            foreach (ISite site in sites)
            {
                if (site.AppType != "Root") continue;
                IEnumerable<SiteWebConfiguration> webConfiguration = Get(site);
                if (webConfiguration != null)
                {
                    siteWebConfigurations.AddRange(webConfiguration);
                }
            }

            if (siteWebConfigurations.Count > 0)
            {
                PackageManager packageManager = PackageManager.GetManager();
                IEnumerable packs = packageManager.GetPacks(siteWebConfigurations);
                foreach (IEnumerable<SiteWebConfiguration> pack in packs)
                {
                    WebTransfer.PostSiteWebConfiguration(pack);
                }
            }
        }
    }
}
