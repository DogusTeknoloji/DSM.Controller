using DSM.Controller.AppServices;
using DSM.Controller.AppServices.IIS;
using DSM.Controller.Shared.Models;
using DSM.Controller.Tracker.Shared;
using DSM.Controller.Tracker.Shared.Models;
using DSM.Core.Interfaces.AppServices;
using DSM.Core.Models;
using DSM.Core.Ops;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DSM.Controller.Tracker.WCF
{
    public partial class TrackerEngine : DSMController
    {
        public TrackerEngine(string authToken) : base(authToken)
        {
        }

        public static void TestAvailability(object @value)
        {
            ISiteEndpoint endpoint = value as ISiteEndpoint;

            Stopwatch watcher = new Stopwatch();

            SiteEndpoint endpointBlock = new SiteEndpoint();
            watcher.Start();
            bool isAvailable = WebOperations
                .CheckUriAvailability(endpoint.EndpointUrl, endpoint.Port, ref endpointBlock);
            watcher.Stop();

            if (endpointBlock != null)
            {
                endpoint.DestinationServer = endpointBlock.DestinationServer;
                endpoint.DestinationAddress = endpointBlock.DestinationAddress;
                endpoint.DestinationAddressType = endpointBlock.DestinationAddressType;
                endpoint.HostInformation = endpointBlock.HostInformation;
                endpoint.HttpProtocol = endpointBlock.HttpProtocol;
                endpoint.IsAvailable = isAvailable;
                endpoint.ServerResponse = endpointBlock.ServerResponse;
                endpoint.ServerResponseDescription = endpointBlock.ServerResponseDescription;
                endpoint.ResponseTime = endpointBlock.ResponseTime;
            }

            watcher.Reset();

            if (MultiThreading.ActiveTaskCounter > 0)
            {
                MultiThreading.ActiveTaskCounter--;
            }
        }
        public static IEnumerable<ISiteEndpoint> TrackEndpoint(IEnumerable<ISite> sites)
        {
            // Create Search Dictionary
            IEnumerable<ISiteEndpoint> dictionaryEndpoints = sites.GetSearchDictionary<ISiteEndpoint>(TrackerType.Endpoint);

            if (dictionaryEndpoints == null) return null;

            // Test Endpoints in the Search Dictionary
            dictionaryEndpoints = dictionaryEndpoints.TestAvailability();

            IList<SiteEndpoint> retvals = new List<SiteEndpoint>();

            foreach (ISite site in sites) // for each sites..do
            {
                //Create list of deleted endpoints.
                List<SiteEndpoint> deletedEndpoints = new List<SiteEndpoint>();

                // Get Endpoints from Database.
                IEnumerable<SiteEndpoint> dbEndpoints = site.GetDBEndpoints();

                // Get Endpoints from live configuration file.
                IEnumerable<SiteEndpoint> currentEndpoints = site.GetEndpoints();


                // If live endpoints is null
                if (dbEndpoints.Count() < 1 && currentEndpoints == null)
                {
                    continue; // skip this iteration
                }

                deletedEndpoints = dbEndpoints.ToList();

                foreach (SiteEndpoint currEndpoint in currentEndpoints) //foreach endpoint..do
                {
                    // Search dictionary test results for site's current endpoint.
                    ISiteEndpoint siteEndpoint = dictionaryEndpoints.FirstOrDefault(dict => dict.EndpointUrl == currEndpoint.EndpointUrl);

                    //pick out the current endpoint from deleted endpoint list.
                    deletedEndpoints = deletedEndpoints.SkipWhile(x => x.EndpointName == currEndpoint.EndpointName
                                                                  && x.EndpointUrl == currEndpoint.EndpointUrl).ToList();
                    if (siteEndpoint == null) // If cannot found any result.
                    {
                        continue; // Skip this iteration.
                    }

                    siteEndpoint.LastCheckDate = DateTime.Now; // Update Checkdate

                    retvals.Add(siteEndpoint as SiteEndpoint); // add this record to return values.

                    // If endpoint is not available and mail status yes
                    if (!siteEndpoint.IsAvailable && siteEndpoint.SendAlertMailWhenUnavailable)
                    {
                        // send mail.
                    }
                } // end of loop..

                //if  some records left in the deleted endpoints -> delete it.
                if (deletedEndpoints.Count() > 0)
                {
                    deletedEndpoints.ForEach(dbep => dbep.DeleteDate = DateTime.Now);
                    WebTransfer.PostSiteEndpoint(dbEndpoints);
                }
            }
            if (retvals.Count() > 0)
            {
                WebTransfer.PostSiteEndpoint(retvals);
            }

            return retvals.AsEnumerable();
        }
        public static void CreateTasks(ref IEnumerable<ISiteEndpoint> endpoints)
        {
            IList<Task> taskList = new List<Task>();
            foreach (ISiteEndpoint endpoint in endpoints)
            {
                TaskFactory factory = new TaskFactory();

                Action<object> operation = new Action<object>(TestAvailability);
                Task task = factory.StartNew(operation, endpoint);

                taskList.Add(task);
            }
        }
        public static void SendAlert(ISiteEndpoint endpoint)
        {
            ISite site = endpoint.Site;
            if (site == null)
            {
                return;
            }
            bool isSiteAvailable = endpoint.Site.State.Swap() == AppServices.IIS.Models.SiteState.Started;

            string appVersionFormatted = $"Bulid {FileOperations.AssemblyVersion}";

            string availablityFormatted;
            string isAvailableFormatted;
            string leftImageContent;
            string titleColorContent;

            if (isSiteAvailable)
            {
                availablityFormatted = "WARNING site is running but the dependency service not resolved";
                isAvailableFormatted = "Yes";
                leftImageContent = MailService.BASE64_WARNING;
                titleColorContent = MailService.TITLE_COLOR_YELLOW;
            }
            else
            {
                availablityFormatted = "CRITICAL CASE! site is not accessible an the dependency service not resolved";
                isAvailableFormatted = "No";
                leftImageContent = MailService.BASE64_CRITICAL_CASE;
                titleColorContent = MailService.TITLE_COLOR_RED;
            }

            MailMessage mailMessage = new MailMessage
            {
                MailTitle = "WCF Service availability test failed!",
                MailSubTitle = endpoint.EndpointUrl,
                MailStatus1 = "WCF SERVICE Connection Check FAILED!",
                MailStatus2 = availablityFormatted,
                MailMachineName = site.MachineName,
                MailSiteUrl = site.Name,
                MailSiteName = site.Name,
                MailSiteAvailable = isAvailableFormatted,
                MailCheckTime = endpoint.LastCheckDate.ToString(),
                MailAppVersion = appVersionFormatted,
                MailLeftImage = leftImageContent,
                MailTitleColor = titleColorContent
            };

            IISMailQueue mail = new IISMailQueue { MailContent = mailMessage.MailContent };
            WebTransfer.PostMail(mail);
        }
    }
}
