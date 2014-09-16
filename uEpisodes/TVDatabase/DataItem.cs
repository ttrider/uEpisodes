using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TTRider.uEpisodes.TVDatabase
{
    class DataItem
    {
        protected object Sync = new object();

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
        protected IEnumerable<List<string>> EnumerateRecords(string data, params string[] fieldNames)
        {
            using (var reader = new StringReader(data))
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
                                yield return new List<string>(relevantFields.Select(index => row[index]));
                            }
                        }
                        line = reader.ReadLine(); 
                    }
                }
            }
        }

    }
}
