using DSM.Core.Models;
using System.Collections.Generic;

namespace DSM.Controller.Tracker.ConnectionString
{
    public class ConnectionStringComparer : IEqualityComparer<SiteConnectionString>
    {
        public bool Equals(SiteConnectionString x, SiteConnectionString y)
        {
            bool fullMatch = x.RawConnectionString == y.RawConnectionString;
            return fullMatch;
        }

        public int GetHashCode(SiteConnectionString obj)
        {
            return obj.RawConnectionString.GetHashCode();
        }
    }
}
