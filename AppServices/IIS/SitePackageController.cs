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
    public class SitePackageController : DSMController,
                                        IDSMSiteAssignable<ISitePackage>
    {
        public SitePackageController(string authToken) : base(authToken)
        {
        }

        public IEnumerable<ISitePackage> Get(ISite site)
        {
            return site.Packages();
        }

        public void Post(ISite site)
        {
            IEnumerable<SitePackage> packages = site.Packages();

            if (packages != null)
            {
                WebTransfer.PostSitePackage(packages);
            }
        }

        public void Post(IEnumerable<ISite> sites)
        {
            List<SitePackage> sitePackages = new List<SitePackage>();
            foreach (ISite site in sites)
            {
                if (site.AppType != "Root") continue;

                IEnumerable<SitePackage> packages = site.Packages();
                if (packages != null)
                {
                    sitePackages.AddRange(packages);
                }
            }

            if (sitePackages.Count > 0)
            {
                PackageManager packageManager = PackageManager.GetManager();
                IEnumerable packs = packageManager.GetPacks(sitePackages);
                foreach (IEnumerable<SitePackage> pack in packs)
                {
                    WebTransfer.PostSitePackage(pack);
                }
            }
        }

    }
}
