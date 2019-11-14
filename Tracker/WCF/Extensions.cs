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

namespace DSM.Controller.Tracker.WCF
{
    public static class Extensions
    {

        /// <summary>
        /// Get endpoints from Site
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static IEnumerable<SiteEndpoint> GetEndpoints(this ISite site)
        {
            IEnumerable<SiteEndpoint> endpoints = site.GetRawEndpoints();

            return endpoints;
        }

        public static IEnumerable<ISiteEndpoint> TestAvailability(this IEnumerable<ISiteEndpoint> endpoints)
        {
            List<ISiteEndpoint> endpointList = new List<ISiteEndpoint>();
            List<Task> epTaskList = new List<Task>();
            foreach (ISiteEndpoint value in endpoints)
            {
                Task task = new Task(new Action(delegate
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

                        endpointList.Add(endpoint);
                    }

                    watcher.Reset();

                    if (MultiThreading.ActiveTaskCounter > 0)
                    {
                        MultiThreading.ActiveTaskCounter--;
                    }

                }));
                epTaskList.Add(task);
            }
            MultiThreading.Run(epTaskList);
            return endpointList.AsEnumerable();
        }
    }
}
