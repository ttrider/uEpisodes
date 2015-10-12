using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TTRider.uEpisodes.TVDatabase
{
    class Show : DataItem
    {
        List<Episode> episodes;
        List<string> wordset;

        public string Title { get; set; }

        public string Directory { get; set; }
        public string TvMaze { get; set; }

        public string TvRage { get; set; }

        public string Country { get; set; }


        public string NormalizedTitle
        {
            get
            {
                return string.Join(" ", this.Wordset);
            }
        }

        public List<string> Wordset
        {
            get { return this.wordset ?? (this.wordset = FileData.GetWordset(this.Title.ToLower()).ToList()); }
        }


        public IEnumerable<string> ExtendedWordset
        {
            get
            {
                foreach (var w in Wordset)
                {
                    yield return w;
                }
                yield return Country.ToLower();
            }
        }


        public IEnumerable<Episode> GetEpisodes(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            lock (this.Sync)
            {

                // if we have TVMaze - use it!
                if (string.IsNullOrWhiteSpace(this.TvMaze))
                {

                    var sf = FileData.Get(new Uri(@"http://epguides.com/common/exportToCSV.asp?rage=" + this.TvRage),
                        this.Directory + ".txt", token);
                    if (sf.FromCache)
                    {
                        if (this.episodes != null)
                        {
                            return this.episodes;
                        }
                    }

                    var data = PreprocessData(sf.Data);

                    this.episodes =
                        new List<Episode>(
                            EnumerateRecords(data, "season", "episode", "title")
                                .Select(item => new Episode(this.Title, item[0], item[1], item[2])));
                    return this.episodes;
                }
                else
                {
                    var sf = FileData.Get(new Uri(@"http://epguides.com/common/exportToCSVmaze.asp?maze=" + this.TvMaze), this.Directory + ".Maze.txt", token);
                    if (sf.FromCache)
                    {
                        if (this.episodes != null)
                        {
                            return this.episodes;
                        }
                    }

                    var data = PreprocessData(sf.Data);

                    this.episodes =
                        new List<Episode>(
                            EnumerateRecords(data, "season", "episode", "title")
                                .Select(item => new Episode(this.Title, item[0], item[1], item[2])));
                    return this.episodes;
                }
            }
        }

        private string PreprocessData(string data)
        {
            var start = data.IndexOf("<pre>", StringComparison.Ordinal) + 5;
            var end = data.IndexOf("</pre>", StringComparison.Ordinal);

            return data.Substring(start, end - start);
        }

        internal Episode GetEpisode(CancellationToken token, int season, int episode)
        {
            return GetEpisodes(token).First(ep => ep.SeasonIndex == season && ep.EpisodeIndex == episode);
        }

        internal bool TryGetEpisode(CancellationToken token, int season, int episode, out Episode episodeInfo)
        {
            token.ThrowIfCancellationRequested();
            episodeInfo = GetEpisodes(token).FirstOrDefault(ep => ep.SeasonIndex == season && ep.EpisodeIndex == episode);
            return episodeInfo != null;
        }
    }
}
