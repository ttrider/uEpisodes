using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TTRider.uEpisodes.Core
{
    public static class DataClient
    {
        private static readonly Lazy<string> CacheDirectory = new Lazy<string>(() =>
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), "TTRider", "epguides");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory;
        });

        private static string UriHash(Uri uri)
        {
            return new Guid(MD5.Create()
                .ComputeHash(
                    Encoding.UTF8.GetBytes(uri.AbsoluteUri)))
                    .ToString("N");
        }

        private static IEnumerable<string> ParseRow(string line)
        {
            var start = 0;
            while (start < line.Length)
            {
                if (line[start] == '\"')
                {
                    var end = line.IndexOf('"', start + 1);
                    if (end == -1)
                    {
                        yield break;
                    }
                    yield return line.Substring(start + 1, end - start - 1);
                    start = end + 2;
                }
                else
                {
                    var end = line.IndexOf(',', start);
                    if (end == -1)
                    {
                        yield return line.Substring(start);
                        yield break;
                    }
                    yield return line.Substring(start, end - start);
                    start = end + 1;
                }
            }
        }


        public static async Task<TextReader> ReadDataAsync(Uri uri, CancellationToken cancellationToken)
        {

            var filePath = Path.Combine(CacheDirectory.Value, "epg_" + UriHash(uri) + ".cache");
            var lastChecked = File.Exists(filePath) ? (DateTime?)File.GetLastWriteTime(filePath) : null;

            if (!lastChecked.HasValue || lastChecked.Value.Date != DateTime.Today)
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.AllowAutoRedirect = true;
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                request.CookieContainer = new CookieContainer();
                request.KeepAlive = true;
                request.Method = "GET";

                if (File.Exists(filePath))
                {
                    request.IfModifiedSince = File.GetCreationTime(filePath);
                }
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11";

                try
                {
                    var response = (HttpWebResponse)await request.GetResponseAsync();
                    if (response != null)
                    {
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var stream = response.GetResponseStream();
                            if (stream != null)
                            {
                                using (var reader = new StreamReader(stream))
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    var buffer = await reader.ReadToEndAsync();
                                    cancellationToken.ThrowIfCancellationRequested();
                                    File.WriteAllText(filePath, buffer);
                                    File.SetCreationTime(filePath, response.LastModified);
                                    File.SetLastWriteTime(filePath, new DateTime(Math.Max(DateTime.Today.Ticks, response.LastModified.Ticks)));

                                    return new StringReader(buffer);
                                }
                            }
                        }
                        finally
                        {
                            response.Close();
                        }
                    }
                    throw new WebException("Can't get response", WebExceptionStatus.PipelineFailure);
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
            cancellationToken.ThrowIfCancellationRequested();

            return File.OpenText(filePath);
        }


        public static async Task ProcessRecordsAsync(TextReader reader, string terminator, Action<string[]> factory, params string[] fieldNames)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            if (factory == null) throw new ArgumentNullException("factory");
            if (fieldNames.Length == 0) throw new ArgumentException("fieldNames");

            var readLine = await reader.ReadLineAsync();
            if (readLine != null)
            {
                var header = readLine;
                while (string.IsNullOrWhiteSpace(header))
                {
                    header = await reader.ReadLineAsync();
                }
                header = header.ToLower();
                var fields = new List<string>(header.Split(',').Select(h => h.Trim()));

                var relevantFields = fieldNames.Select(name => fields.IndexOf(name.ToLower())).ToList();

                string line = await reader.ReadLineAsync();
                while (line != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (terminator != null && line.Contains(terminator))
                        {
                            break;
                        }

                        var row = ParseRow(line).ToList();

                        if (row.Count == fields.Count)
                        {
                            factory(relevantFields.Select(index => row[index]).ToArray());
                        }
                    }
                    line = await reader.ReadLineAsync();
                }
            }
        }

        public static IEnumerable<string> GetWordset(string name)
        {
            if (name == null) name = "";
            return name.Split(new[] { ' ', '\t', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '+', '=', '`', '~', '\'', '|', '\\', '[', '{', '}', ']', ',', '.', '<', '>', '/', '?', ';', ':', '"' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.ToLower())
                .OrderBy(w => w)
                .Where(w => w.Length > 1)
                .Where(w => w != "the")
                .Where(w => w != "of")
                .Where(w => w != "in")
                .Where(w => w != "at")
                .Distinct();
        }

        public static async Task SkipUntilAsync(TextReader reader, string token)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            if (token == null) return;

            var charSet = token.ToCharArray();

            var buffer = new char[charSet.Length];
            var actual = await reader.ReadAsync(buffer, 0, charSet.Length);
            if (actual != charSet.Length)
            {
                return;
            }
            do
            {
                if (buffer.SequenceEqual(charSet))
                {
                    return;
                }
                Array.Copy(buffer, 1, buffer, 0, charSet.Length - 1);
                actual = await reader.ReadAsync(buffer, charSet.Length - 1, 1);
            } while (actual == 1);
        }
    }
}
