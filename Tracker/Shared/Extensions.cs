using DSM.Controller.Tracker.ConnectionString;
using DSM.Controller.Tracker.WCF;
using DSM.Core.Interfaces.AppServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace DSM.Controller.Tracker.Shared
{
    public static class Extensions
    {
        /// <summary>
        /// Bulid and get search dictionary from sites
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sites"></param>
        /// <param name="dictionaryType"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetSearchDictionary<T>(this IEnumerable<ISite> sites, TrackerType dictionaryType)
        {
            IEnumerable<T> results = new T[0];

            foreach (ISite site in sites)
            {
                dynamic db = null;
                switch (dictionaryType)
                {
                    case TrackerType.Endpoint:
                        db = site.GetEndpoints();
                        break;
                    case TrackerType.ConnectionString:
                        db = site.GetDBConnectionStrings();
                        break;
                    default:
                        break;
                }
                if (db != null && ((IEnumerable<T>)db).Count() > 0)
                {
                    results = results.Union((IEnumerable<T>)db);
                    if (dictionaryType == TrackerType.Endpoint)
                    {
                        results = results.Distinct(new EndpointComparer() as IEqualityComparer<T>);
                    }
                    else if (dictionaryType == TrackerType.ConnectionString)
                    {
                        results = results.Distinct(new ConnectionStringComparer() as IEqualityComparer<T>);
                    }
                }
            }
            return results;
        }

        public static Queue<T> Reload<T>(this Queue<T> @value, IList<T> items)
        {
            T[] queueItems = value.ToArray();
            IEnumerable<T> joinList = queueItems.Concat(items);

            return new Queue<T>(joinList);
        }
    }

}
