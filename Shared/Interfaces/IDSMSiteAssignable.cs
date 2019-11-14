using DSM.Core.Interfaces.AppServices;
using System.Collections.Generic;

namespace DSM.Controller.Shared.Interfaces
{
    public interface IDSMSiteAssignable<T>
    {
        IEnumerable<T> Get(ISite site);
        void Post(ISite site);
        void Post(IEnumerable<ISite> sites);
    }
}
