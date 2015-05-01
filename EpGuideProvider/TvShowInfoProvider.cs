using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EpGuideProvider
{
    public class TvShowInfoProvider
    {
        const string showListUri = @"http://epguides.com/common/allshows.txt";
        const string showUri = @"http://epguides.com/common/exportToCSV.asp?rage=";


        public TvShowInfoProvider()
        {
        }

        string LocalCacheDirectory
        {
            get
            {
                var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TTRider", "uShowRenamer");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                return directory;
            }
        }

        Task<StreamInfo> GetStreamInfoAsync(Uri uri, CancellationToken token)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            return GetStreamInfoAsync(uri, uri.Segments.Last(), token);
        }
        async Task<StreamInfo> GetStreamInfoAsync(Uri uri, string localFileName, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TTRider", "uShowRenamer");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var filePath = Path.Combine(directory, localFileName);

        }


        private static IEnumerable<string> ParseCsvRow(string line)
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

        private static IEnumerable<dynamic> ParseCsvStream(TextReader reader, params string[] fieldNames)
        {
            var readLine = reader.ReadLine();
            if (readLine != null)
            {
                var header = readLine;
                while (string.IsNullOrWhiteSpace(header))
                {
                    header = reader.ReadLine();
                }
                header = header.ToLower();
                var fields = new List<string>(header.Split(','));

                var relevantFields = fieldNames.Select(name => fields.IndexOf(name.ToLower())).ToList();

                string line = reader.ReadLine();
                while (line != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var row = ParseRow(line).ToList();

                        if (row.Count == fields.Count)
                        {
                            dynamic ret = new ExpandoObject();

                            for (int i = 0; i < relevantFields.Length; i++)
                            {
                                ret[fieldNames[relevantFields[i]]] = row[i];
                            }

                            yield return ret;
                        }
                    }
                    line = reader.ReadLine();
                }
            }

        }
    }
}
