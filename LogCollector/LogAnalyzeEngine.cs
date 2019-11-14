using DSM.Core.Interfaces.AppServices;
using DSM.Core.Models;
using DSM.Core.Ops;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DSM.Controller.LogCollector
{
    public class LogAnalyzeEngine
    {
        private readonly StreamReader logFileReader = null;
        private readonly MemoryStream logFile = new MemoryStream();

        private const string fields = "#Fields: ";
        private readonly Dictionary<string, string> logFileDictionary = new Dictionary<string, string>();
        private readonly Dictionary<int, string> logIndexDictionary = new Dictionary<int, string>();

        public LogAnalyzeEngine(string logPath, DateTime lastLogDate = default(DateTime))
        {
            string fullLogPath = GetLastFullLogPath(logPath, lastLogDate);
            if (fullLogPath == null)
            {
                return;
            }
            GetFileContent(logPath, fullLogPath);
            logFileReader = new StreamReader(logFile);

            InitializeFileDictionary(); // Match log file and class fields.
        }

        private void GetFileContent(string logPath, string fullLogPath)
        {
            string logName = Path.GetTempFileName();
            string newDir = Path.Combine(logPath, "", logName);
            newDir.AutoPathRepair();
            File.Copy(fullLogPath, newDir, overwrite: true);
            FileStream fileStream = new FileStream(newDir, FileMode.Open);

            fileStream.CopyTo(logFile);
            fileStream.Close();
            fileStream.Dispose();
            logFile.Position = 0;
            File.Delete(newDir);
        }

        private void InitializeFileDictionary()
        {
            logFileDictionary.Clear();
            logFileDictionary.Add("date", "RLDate");
            logFileDictionary.Add("time", "RLTime");
            logFileDictionary.Add("s-sitename", "ServerSiteName");
            logFileDictionary.Add("s-computername", "ServerComputerName");
            logFileDictionary.Add("s-ip", "ServerIp");
            logFileDictionary.Add("cs-method", "RequestMethod");
            logFileDictionary.Add("cs-uri-stem", "RequestUri");
            logFileDictionary.Add("cs-uri-query", "RequestUriQuery");
            logFileDictionary.Add("s-port", "ServerPort");
            logFileDictionary.Add("cs-username", "RequestUsername");
            logFileDictionary.Add("c-ip", "RequestedIp");
            logFileDictionary.Add("cs-version", "RequestBrowserVersion");
            logFileDictionary.Add("cs(User-Agent)", "RequestUserAgent");
            logFileDictionary.Add("cs(Cookie)", "RequestCookie");
            logFileDictionary.Add("cs(Referer)", "RequestReferer");
            logFileDictionary.Add("cs-host", "RequestHost");
            logFileDictionary.Add("sc-status", "ServiceStatus");
            logFileDictionary.Add("sc-substatus", "ServiceSubStatus");
            logFileDictionary.Add("sc-win32-status", "ServiceWin32Status");
            logFileDictionary.Add("sc-bytes", "ServiceTransferedBytes");
            logFileDictionary.Add("cs-bytes", "RequestTransferedBytes");
            logFileDictionary.Add("time-taken", "RequestTimeTakenMiliSeconds");

        }

        private void InitializeIndexDictionary(string fields)
        {
            logIndexDictionary.Clear();
            string[] colHeaders = fields.Split(' ');
            for (int i = 0; i < colHeaders.Length; i++)
            {
                string item = colHeaders[i];
                string propertyName = logFileDictionary[item];
                logIndexDictionary.Add(i, propertyName);
            }
        }

        private ISiteTransaction ParseLine(string line)
        {
            ISiteTransaction transaction = new SiteTransaction();
            string[] data = line.Split(' ');

            for (int i = 0; i < data.Length; i++)
            {
                string currentProperty = logIndexDictionary[i]; // Get property for this Index;
                Type IISLDT = typeof(SiteTransaction);

                PropertyInfo[] properties = IISLDT.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.Name == currentProperty)
                    {
                        object value = Convert.ChangeType(data[i], property.PropertyType);
                        property.SetValue(transaction, value);
                        break;
                    }
                }
            }
            return transaction;
        }

        private ISiteTransaction FilterLine(ISiteTransaction transaction, IEnumerable<SiteTransactionFilterExcludedItem> siteTransactionFilters)
        {
            string uri = transaction.RequestUri;
            foreach (SiteTransactionFilterExcludedItem siteTransactionFilter in siteTransactionFilters)
            {
                if (uri.Contains(siteTransactionFilter.Name))
                {
                    return null;
                }
            }
            return transaction;
        }
        private string GetLastFullLogPath(string relativePath, DateTime lastLogDate)
        {
            if (lastLogDate == default(DateTime))
            {
                lastLogDate = DateTime.Now;
            }

            bool flag = true;
            DateTime startDate = DateTime.Now.Date;
            string calculatedLogPath = string.Empty;
            string fullPath = string.Empty;
            double difference = 0.0f;

            while (flag)
            {
                calculatedLogPath = $"u_ex{startDate.ToString("yyMMdd")}.log";
                fullPath = Path.Combine(relativePath, calculatedLogPath);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }

                difference = (DateTime.Now - startDate).TotalDays;

                if (difference > 30)
                {
                    return null;
                }

                startDate = startDate.AddDays(-1);
            }

            return null;
        }

        public IEnumerable<ISiteTransaction> GetLogs(ISite site, DateTime lastLogDate, IEnumerable<SiteTransactionFilterExcludedItem> filterExcludedItems = null)
        {
            if (logFile == null)
            {
                return null;
            }

            IList<ISiteTransaction> logs = new List<ISiteTransaction>();

            while (logFileReader.Peek() > 0)
            {
                string line = logFileReader.ReadLine();
                if (line.StartsWith("#") && !line.StartsWith(fields))
                {
                    continue;
                }

                if (line.StartsWith(fields))
                {
                    string colHeadersRaw = line.Substring(fields.Length, line.Length - fields.Length);
                    InitializeIndexDictionary(colHeadersRaw);
                }
                else
                {
                    if (logIndexDictionary.Count < 1)
                    {
                        return null;
                    }

                    ISiteTransaction parsedLine = ParseLine(line);
                    if (filterExcludedItems != null)
                    {
                        parsedLine = FilterLine(parsedLine, filterExcludedItems);
                    }

                    if (parsedLine?.LogDate <= lastLogDate)
                    {
                        parsedLine = null;
                    }

                    if (parsedLine != null)
                    {
                        parsedLine.SiteId = site.Id;
                        logs.Add(parsedLine);
                    }
                }
            }

            logFile.Close();
            logFile.Dispose();
            return logs;
        }

    }
}
