using DSM.Controller.Shared.Interfaces;
using DSM.Core.Ops;

namespace DSM.Controller.Shared.Models
{
    public class DSMController : IDSMAuthenticable
    {
        protected LogManager logManager = LogManager.GetManager("DSM.Controller.Shared");

        public DSMController(string authToken)
        {
            WebTransfer.Authorize(authToken);
        }
    }
}
