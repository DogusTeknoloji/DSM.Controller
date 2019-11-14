using DSM.Core.Models;
using System;
using System.Collections.Generic;

namespace DSM.Controller.Tracker.WCF
{
    public class EndpointComparer : IEqualityComparer<SiteEndpoint>
    {
        public bool Equals(SiteEndpoint x, SiteEndpoint y)
        {
            bool endpointUrlEquality = string.Equals(x.EndpointUrl, y.EndpointUrl, StringComparison.OrdinalIgnoreCase);
            bool portEquality = string.Equals(x.Port.ToString(), y.Port.ToString(), StringComparison.OrdinalIgnoreCase);

            return endpointUrlEquality && portEquality;
        }

        public int GetHashCode(SiteEndpoint obj)
        {
            return obj.EndpointUrl.GetHashCode();
        }
    }
}
