using DSM.Controller.Shared.Models;
using DSM.Core.Models;
using DSM.Core.Ops;
using System;
using System.Collections.Generic;

namespace DSM.Controller.Tracker.Shared
{
    public class TrackerEngine : DSMController
    {
        private readonly TrackerType _trackerType;
        public TrackerEngine(string authToken, TrackerType trackerType) : base(authToken)
        {
            _trackerType = trackerType;
        }

        public void PostTrackerValues()
        {
            IEnumerable<Site> sites = WebTransfer.GetSites(Environment.MachineName);
            if (_trackerType == TrackerType.Endpoint)
            {
                WCF.TrackerEngine.TrackEndpoint(sites);
            }
            else if (_trackerType == TrackerType.ConnectionString)
            {
                ConnectionString.TrackerEngine.TrackConnectionString(sites);
            }
        }
    }
}
