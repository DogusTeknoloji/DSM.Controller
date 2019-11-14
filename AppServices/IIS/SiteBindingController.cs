using DSM.Controller.Shared.Interfaces;
using DSM.Controller.Shared.Models;
using DSM.Core.Interfaces.AppServices;
using DSM.Core.Models;
using DSM.Core.Ops;
using DSM.Core.PackageManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DSM.Controller.AppServices.IIS
{
    public class SiteBindingController : DSMController,
                                        IDSMSiteAssignable<SiteBinding>
    {
        public SiteBindingController(string authToken) : base(authToken)
        {
        }

        public IEnumerable<SiteBinding> Get(ISite site)
        {
            return site.Bindings();
        }

        public void Post(ISite site)
        {
            IEnumerable<SiteBinding> bindings = site.Bindings();
            WebTransfer.PostSiteBinding(bindings);
        }

        public void Post(IEnumerable<ISite> sites)
        {
            List<SiteBinding> siteBindings = new List<SiteBinding>();
            foreach (ISite site in sites)
            {
                if (site.AppType != "Root") continue;

                IEnumerable<SiteBinding> bindings = site.Bindings();
                string xConsoleString = string.Join(",", bindings.Select(x => $"SiteId:{x.SiteId} Host:{x.Host} Port:{x.Port} SSL:{x.IsSSLBound}"));
                XConsole.WriteLine(xConsoleString);
                siteBindings.AddRange(bindings);
            }

            if (siteBindings.Count > 0)
            {
                PackageManager packageManager = PackageManager.GetManager();
                IEnumerable packs = packageManager.GetPacks(siteBindings);
                foreach (IEnumerable<SiteBinding> pack in packs)
                {
                    WebTransfer.PostSiteBinding(pack);
                }
            }

        }
    }
}
