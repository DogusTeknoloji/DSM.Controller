using DSM.Core.Interfaces.AppServices;
using DSM.Core.Interfaces.LogServices;
using DSM.Core.Models.LogServices;
using DSM.Core.Ops;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Tx.Windows;

namespace DSM.Controller.LogCollector.IIS
{
    public static class LogAnalyzeEngine
    {
        public static Stream LoadFileContent(string logFolderPath)
        {
            MemoryStream logStream = new MemoryStream();
            string fullLogPath = GetLogFileName(logFolderPath);

            if (fullLogPath == null)
            {
                return null;
            }

            string logName = Path.GetTempFileName();
            string newDir = Path.Combine(logFolderPath, "", logName);
            newDir.AutoPathRepair();
            File.Copy(fullLogPath, newDir, overwrite: true);
            FileStream fileStream = new FileStream(newDir, FileMode.Open);

            fileStream.CopyTo(logStream);
            fileStream.Close();

            logStream.Position = 0;

            File.Delete(newDir);

            return logStream;
        }
        private static long GetObjectSize(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            long size = ms.Length;
            ms.Dispose();
            return size;
        }
        public static long SizeBytes(this ISiteLog log)
        {
            return GetObjectSize(log);
        }
        public static long SizeBytes(this IEnumerable<ISiteLog> logs)
        {
            ISiteLog log = logs.FirstOrDefault();
            long sizeOfBytes = log.SizeBytes();
            int count = logs.Count();
            long result = sizeOfBytes * count;
            return result;
        }
        public static void ExcludeMediaItems(ref IEnumerable<W3CEvent> logs)
        {
            logs = logs.Where(x =>
                                   !x.cs_uri_stem.ToLower().Contains(".js")
                                && !x.cs_uri_stem.ToLower().Contains(".css")
                                && !x.cs_uri_stem.ToLower().Contains(".png")
                                && !x.cs_uri_stem.ToLower().Contains(".jpg")
                                && !x.cs_uri_stem.ToLower().Contains(".jpeg")
                                && !x.cs_uri_stem.ToLower().Contains(".gif")
                                && !x.cs_uri_stem.ToLower().Contains(".ttf")
                                && !x.cs_uri_stem.ToLower().Contains(".woff")
                                && !x.cs_uri_stem.ToLower().Contains(".woff2")
                                && !x.cs_uri_stem.ToLower().Contains(".eot")
                                && !x.cs_uri_stem.ToLower().Contains(".svg")
                                && !x.cs_uri_stem.ToLower().Contains(".ico"));
        }
        public static string GetLogFileName(this string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return null;
            }
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            IEnumerable<FileInfo> files = dirInfo.GetFiles().AsEnumerable().OrderByDescending(x => x.LastWriteTime);
            FileInfo file = files.FirstOrDefault();

            return file.FullName;
        }
        public static IEnumerable<ISiteLog> GetLogs(string logFolderPath)
        {
            Stream contentStream = LoadFileContent(logFolderPath);
            if (contentStream == null)
            {
                return null;
            }

            StreamReader reader = new StreamReader(contentStream);
            IEnumerable<W3CEvent> events = W3CEnumerable.FromStream(reader);

            ExcludeMediaItems(ref events); // Exclude js,css,image,font items from logs

            IEnumerable<ISiteLog> logs = events.Select(x => new SiteLog
            {
                LogDate = x.dateTime,

                ServerSiteName = x.s_sitename,
                ServerComputerName = x.s_computername,
                ServerIp = x.s_ip,
                ServerPort = x.s_port,
                ServerReceivedBytes = x.cs_bytes,
                ServerResponseCode = x.sc_status,
                ServerWin32Code = x.sc_win32_status,
                ServerResponseSubStatus = x.sc_substatus,
                ServerResponseTimeMiliseconds = x.time_taken,
                ServerSentBytes = x.sc_bytes,

                ClientRequestedMethod = x.cs_method,
                ClientRequestedUri = x.cs_uri_stem,
                ClientRequestedUriQuery = x.cs_uri_query,
                ClientUserName = x.cs_username,
                ClientIp = x.c_ip,
                ClientBrowserVersion = x.cs_version,
                ClientUserAgent = x.cs_User_Agent,
                ClientRequestedCookie = x.cs_Cookie,
                ClientRequestedHost = x.cs_host,
                ClientRequestedReferer = x.cs_Referer
            });

            return logs;
        }
        public static IEnumerable<IEnumerable<ISiteLog>> GetPacks(IEnumerable<ISiteLog> logs)
        {
            IList<IEnumerable<ISiteLog>> packs = new List<IEnumerable<ISiteLog>>();

            long totalAllowedBytes = 5 * FileSize.MB;
            long sizeofBytes = logs.SizeBytes();
            long packCount = (sizeofBytes / (int)totalAllowedBytes) + 1;
            int count = logs.Count() / (int)packCount;

            for (int i = 0; i < packCount; i++)
            {
                IEnumerable<ISiteLog> logPack = logs.Take(count);
                packs.Add(logPack);
            }
            return packs;
        }
        public static ISiteLogPosition GetLastPosition(ISite site)
        {
            ISiteLogPosition position = WebTransfer.GetLogPosition(site.Id);
            return position;
        }
    }
}