using DSM.Controller.AppServices;
using DSM.Controller.Tracker.Shared;
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
    public static class Extensions
    {
        private static LogManager logManager = LogManager.GetManager("DSM.Controller.Tracker.ConnectionString");
        public static IEnumerable<SiteConnectionString> GetConnectionStrings(this ISite site)
        {
            IEnumerable<SiteConnectionString> connectionStrings = site.GetRawConnectionStrings();

            return connectionStrings;
        }

        public static IEnumerable<ISiteConnectionString> TestAvaiilabilty(this IEnumerable<ISiteConnectionString> connectionStrings)
        {
            List<ISiteConnectionString> connectionstringList = new List<ISiteConnectionString>();
            List<Task> csTaskList = new List<Task>();
            foreach (ISiteConnectionString value in connectionStrings)
            {
                Task task = new Task(new Action(delegate
                {
                    try
                    {
                        ISiteConnectionString connectionString = value;
                        Stopwatch watcher = new Stopwatch();

                        watcher.Start();
                        XConsole.WriteLine("CheckSQLDatabaseAccesibility:" + connectionString.RawConnectionString);
                        bool isAvailable = WebOperations.CheckSQLDatabaseAccesibility(connectionString?.RawConnectionString);
                        watcher.Stop();

                        connectionString.IsAvailable = isAvailable;
                        connectionString.ResponseTime = watcher.ElapsedMilliseconds;
                        connectionstringList.Add(connectionString);

                        watcher.Reset();

                        if (MultiThreading.ActiveTaskCounter > 0)
                        {
                            MultiThreading.ActiveTaskCounter--;
                        }
                    }
                    catch (Exception ex)
                    {
                        XConsole.WriteLine(ex.ToString());
                        logManager.Write(ex.ToString());
                    }
                }));
                csTaskList.Add(task);
            }
            MultiThreading.Run(csTaskList);
            return connectionstringList.AsEnumerable();
        }
    }
}
