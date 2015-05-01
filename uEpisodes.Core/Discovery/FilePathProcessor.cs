using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TTRider.uEpisodes.Core.Discovery
{
    public class FilePathProcessor
    {
        private readonly List<Regex> patternSet = new List<Regex>();
        private readonly HashSet<string> excludeDirectorySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public void Initialize(IEnumerable<string> patterns, IEnumerable<string> excludeDirectories)
        {
            this.patternSet.Clear();
            this.excludeDirectorySet.Clear();

            if (patterns != null)
            {
                this.patternSet.AddRange(
                    patterns.Select(p => new Regex(p, RegexOptions.Compiled | RegexOptions.IgnoreCase)));
            }

            if (excludeDirectories != null)
            {
                foreach (var excludeDirectory in excludeDirectories)
                {
                    this.excludeDirectorySet.Add(excludeDirectory);
                }
            }
        }


        public VideoFileInfo ProcessFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path");

            var vfi = new VideoFileInfo(path);

            if (vfi.FileParts.Any(fp => this.excludeDirectorySet.Contains(fp)))
            {
                return vfi;
            }

            foreach (var pattern in this.patternSet)
            {
                var match = pattern.Match(vfi.FileName);
                if (match.Success)
                {
                    var vid = new VideoId();
                    var success = false;
                    if (match.Groups["show"].Success)
                    {
                        vid.Show = match.Groups["show"].Value.Trim();
                        success = true;
                    }
                    if (match.Groups["season"].Success)
                    {
                        int value;
                        if (int.TryParse(match.Groups["season"].Value.Trim(), out value))
                        {
                            vid.Season = value;
                            success = true;
                        }
                    }
                    if (match.Groups["episode"].Success)
                    {
                        int value;
                        if (int.TryParse(match.Groups["episode"].Value.Trim(), out value))
                        {
                            vid.Episode = value;
                            success = true;
                        }
                    }

                    if (success)
                    {
                        vfi.VideoIdSet.Add(vid);
                    }
                }
            }
            return vfi;
        }
    }

}
