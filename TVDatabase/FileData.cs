using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace TTRider.uEpisodes.TVDatabase
{
    class FileData
    {
        string filePath;
        string data;

        public string Data
        {
            get { return data ?? (data = File.ReadAllText(this.filePath)); }
        }
        public bool FromCache
        {
            get;
            private set;
        }

        public static FileData Get(Uri uri, CancellationToken token)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            return Get(uri, uri.Segments.Last(), token);
        }

        public static FileData Get(Uri uri, string localFileName, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var ret = new FileData();

            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TTRider", "uShowRenamer");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var filePath = Path.Combine(directory, localFileName);
            ret.filePath = filePath;

            var lastChecked = File.Exists(filePath) ? (DateTime?)File.GetLastWriteTime(filePath) : null;

            if (!lastChecked.HasValue || lastChecked.Value.Date != DateTime.Today)
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.AllowAutoRedirect = true;
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                request.CookieContainer = new CookieContainer();
                request.KeepAlive = true;
                request.Method = "GET";

                token.Register(request.Abort);

                if (File.Exists(filePath))
                {
                    request.IfModifiedSince = File.GetCreationTime(filePath);
                }
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11";

                try
                {
                    var response = (HttpWebResponse)request.GetResponse();

                    try
                    {
                        token.ThrowIfCancellationRequested();
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            token.ThrowIfCancellationRequested();
                            var buffer = reader.ReadToEnd();
                            token.ThrowIfCancellationRequested();
                            File.WriteAllText(filePath, buffer);
                            File.SetCreationTime(filePath, response.LastModified);
                            File.SetLastWriteTime(filePath, new DateTime(Math.Max(DateTime.Today.Ticks, response.LastModified.Ticks)));

                            ret.data = buffer;
                            return ret;
                        }
                    }
                    finally
                    {
                        response.Close();
                    }
                }
                catch (WebException ex)
                {
                    if ((ex.Status != WebExceptionStatus.ProtocolError) ||
                        (((HttpWebResponse)ex.Response).StatusCode != HttpStatusCode.NotModified))
                    {
                        throw;
                    }
                    File.SetLastWriteTime(filePath, new DateTime(Math.Max(DateTime.Today.Ticks, File.GetCreationTime(filePath).Ticks)));
                }
            }
            token.ThrowIfCancellationRequested();
            ret.FromCache = true;
            return ret;
        }

        public static IEnumerable<string> GetWordset(string name)
        {
            if (name == null) name = "";
            return name.Split(new[] { ' ', '\t', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '+', '=', '`', '~', '\'', '|', '\\', '[', '{', '}', ']', ',', '.', '<', '>', '/', '?', ';', ':', '"' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.ToLower());
        }
    }
}
