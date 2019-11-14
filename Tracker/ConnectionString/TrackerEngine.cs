using DSM.Controller.AppServices;
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

namespace DSM.Controller.Tracker.ConnectionString
{
    public partial class TrackerEngine : DSMController
    {
        private new static readonly LogManager logManager = LogManager.GetManager("DSM.Controller.Tracker.ConnectionString");
        public TrackerEngine(string authToken) : base(authToken)
        {

        }

        public static void TestAvailability(object @value)
        {
            ISiteConnectionString connectionString = value as ISiteConnectionString;

            Stopwatch watcher = new Stopwatch();
            SiteConnectionString connectionStringBlock = new SiteConnectionString();

            watcher.Start();
            bool isAvailable = WebOperations
                .CheckSQLDatabaseAccesibility(connectionString.RawConnectionString);
            watcher.Stop();
            connectionStringBlock.IsAvailable = isAvailable;
            connectionStringBlock.LastCheckTime = DateTime.Now;
            connectionStringBlock.ResponseTime = watcher.ElapsedMilliseconds;

            watcher.Reset();

            if (MultiThreading.ActiveTaskCounter > 0)
            {
                MultiThreading.ActiveTaskCounter--;
            }
        }
        public static IEnumerable<ISiteConnectionString> TrackConnectionString(IEnumerable<ISite> sites)
        {
            // Create Search Dictionary
            IEnumerable<ISiteConnectionString> dictionaryConnectionStrings = sites.GetSearchDictionary<ISiteConnectionString>(TrackerType.ConnectionString);

            if (dictionaryConnectionStrings == null)
            {
                return null;
            }

            // Test ConnectionStrings in the Search Dictionary
            dictionaryConnectionStrings = dictionaryConnectionStrings.TestAvaiilabilty();

            IList<SiteConnectionString> retvals = new List<SiteConnectionString>();

            foreach (ISite site in sites) // for each sites..do
            {
                // Create list of deleted connection strings
                List<SiteConnectionString> deletedConnectionstrings = new List<SiteConnectionString>();

                // Get Connectionstrings from database.
                IEnumerable<SiteConnectionString> dbConnectionstrings = site.GetDBConnectionStrings();

                // Get Connectionstrings from live configuration file.
                IEnumerable<SiteConnectionString> currentConnectionstrings = site.GetConnectionStrings();


                // If live connectionstrings is null
                if (dbConnectionstrings.Count() < 1 && currentConnectionstrings == null)
                {
                    continue; // skip this iteration
                }

                deletedConnectionstrings = dbConnectionstrings.ToList();

                foreach (SiteConnectionString currConnectionstring in currentConnectionstrings) //foreach connectionstring
                {
                    // Search dictionary test results for site's current connectionstring
                    ISiteConnectionString siteConnectionString = dictionaryConnectionStrings.FirstOrDefault(dict => dict.RawConnectionString == currConnectionstring.RawConnectionString);

                    deletedConnectionstrings = deletedConnectionstrings.SkipWhile(x => x.ConnectionName == currConnectionstring.ConnectionName
                                                                                 && x.RawConnectionString == currConnectionstring.RawConnectionString).ToList();

                    if (siteConnectionString == null) // If cannot found any result.
                    {
                        continue; // Skip this iteration.
                    }

                    currConnectionstring.LastCheckTime = DateTime.Now; // Update Checkdate

                    retvals.Add(siteConnectionString as SiteConnectionString); // add this record to return values.

                    // If Connectionstring is not available and  mail status yes
                    if (!currConnectionstring.IsAvailable && currConnectionstring.SendAlertMailWhenUnavailable)
                    {
                        // send mail
                    }
                } // end of loop.

                if (deletedConnectionstrings.Count() > 0)
                {
                    deletedConnectionstrings.ForEach(dbcs => dbcs.DeleteDate = DateTime.Now);
                    WebTransfer.PostSiteConnectionString(retvals);
                }
            } // end of loop.

            if (retvals.Count() > 0)
            {
                WebTransfer.PostSiteConnectionString(retvals);
            }

            return retvals.AsEnumerable();
        }

        public static void CreateTasks(ref IEnumerable<ISiteConnectionString> connectionStrings)
        {
            IList<Task> taskList = new List<Task>();
            foreach (ISiteConnectionString connectionString in connectionStrings)
            {
                TaskFactory factory = new TaskFactory();

                Action<object> operation = new Action<object>(TestAvailability);
                Task task = factory.StartNew(operation, connectionString);

                taskList.Add(task);
            }
        }

        public static void SendAlert(ISiteConnectionString connectionString)
        {
            ISite site = connectionString.Site;
            if (site == null)
            {
                return;
            }
            bool isSiteAvailable = connectionString.Site.State.Swap() == AppServices.IIS.Models.SiteState.Started;

            string statusFormatted = connectionString.ServerName + "," + connectionString.Port + "&" + connectionString.DatabaseName;
            string appVersionFormatted = $"Bulid {FileOperations.AssemblyVersion}";

            string availabilityFormatted;
            string isAvailableFormatted;
            string leftImageContent;
            string titleColorContent;

            if (isSiteAvailable)
            {
                availabilityFormatted = "WARNING! site is running but the dependency service not resolved.";
                isAvailableFormatted = "Yes";
                leftImageContent = MailService.BASE64_WARNING;
                titleColorContent = MailService.TITLE_COLOR_YELLOW;
            }
            else
            {
                availabilityFormatted = "CRITICAL CASE! site is not accessible and the dependency service not resolved.";
                isAvailableFormatted = "No";
                leftImageContent = MailService.BASE64_CRITICAL_CASE;
                titleColorContent = MailService.TITLE_COLOR_RED;
            }

            MailMessage mailMessage = new MailMessage
            {
                MailTitle = "ConnectionString Availability test failed!",
                MailSubTitle = statusFormatted,
                MailStatus1 = "DATABASE Connection Check FAILED!",
                MailStatus2 = availabilityFormatted,
                MailMachineName = site.MachineName,
                MailSiteUrl = site.Name,
                MailSiteName = site.Name,
                MailSiteAvailable = isAvailableFormatted,
                MailCheckTime = connectionString.LastCheckTime.ToString(),
                MailAppVersion = appVersionFormatted,
                MailLeftImage = leftImageContent,
                MailTitleColor = titleColorContent
            };

            IISMailQueue mail = new IISMailQueue { MailContent = mailMessage.MailContent };
            WebTransfer.PostMail(mail);
        }
    }
}
