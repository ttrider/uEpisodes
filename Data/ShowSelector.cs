using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TTRider.uEpisodes.TVDatabase;

namespace TTRider.uEpisodes.Data
{
    class ShowSelector : PropertyStore
    {
        static readonly Regex episodeIndexRegex = new Regex(@"(S(?<s>[0-9]+)E(?<e>[0-9]+))|((?<s>[0-9]+)x(?<e>[0-9]+))|(Season\s*(?<s>[0-9]+)\s*Episode\s*(?<e>[0-9]+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex episodeIndexAltRegex = new Regex(@"[0-9]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        private readonly EpisodeFile episodeFile;
        private CancellationTokenSource cancellation = new CancellationTokenSource();


        public ShowSelector(EpisodeFile episodeFile)
        {
            this.episodeFile = episodeFile;
            this.EpisodeSelection = new DispatchedCollection<ShowEpisodeItem>();
            this.EpisodeSelection.CollectionChanged += (s, e) => OnPropertyChanged("EpisodeSelection");

        }

        /// <summary>
        /// 
        /// </summary>
        public DispatchedCollection<ShowEpisodeItem> EpisodeSelection
        {
            get;
            private set;
        }

        #region Properties

        private string searchString;
        /// <summary>
        /// String for manual Show search
        /// </summary>
        public string SearchString
        {
            get { return this.searchString; }
            set { this.SetValue(ref searchString, value, "SearchString"); }
        }

        private bool inAutomaticMode;
        /// <summary>
        /// Indicates that control doesn't need user interractions
        /// </summary>
        public bool InAutomaticMode
        {
            get { return this.inAutomaticMode; }
            set { this.SetValue(ref inAutomaticMode, value, "InAutomaticMode"); }
        }

        private bool inSearchingMode;
        /// <summary>
        /// Indicates that we are currently searching for a show
        /// </summary>
        public bool InSearchingMode
        {
            get { return this.inSearchingMode; }
            set { this.SetValue(ref inSearchingMode, value, "InSearchingMode"); }
        }

        public bool IsReady
        {
            get { return this.episodeFile.IsReady; }
            set
            {
                if (value != IsReady)
                {
                    this.episodeFile.IsReady = value;
                    OnPropertyChanged("IsReady");
                }
            }
        }

        private string status;
        /// <summary>
        /// Status Message
        /// </summary>
        public string Status
        {
            get { return this.status; }
            set { this.SetValue(ref status, value, "Status"); }
        }

        private bool isCancelling;
        /// <summary>
        ///  
        /// </summary>
        public bool IsCancelling
        {
            get { return this.isCancelling; }
            set { this.SetValue(ref isCancelling, value, "IsCancelling"); }
        }


        #endregion Properties

        public void DoSearch()
        {
            if (!string.IsNullOrWhiteSpace(SearchString) && !this.InSearchingMode)
            {
                this.Status = "Search pending ...";
                this.IsCancelling = false;
                this.InSearchingMode = true;
                this.cancellation = new CancellationTokenSource();

                Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            cancellation.Token.ThrowIfCancellationRequested();
                            this.Status = "Searching ...";
                            var episodes = Shows.Current.GetShowEpisode(SearchString.Trim(),
                                                                        this.episodeFile.Season.GetValueOrDefault(),
                                                                        this.episodeFile.Episode.GetValueOrDefault(),
                                                                        this.cancellation.Token);

                            cancellation.Token.ThrowIfCancellationRequested();
                            this.Status = "Processing Episodes ...";
                            if (episodes != null)
                            {
                                var newEpisodeItems = episodes.Select(ef => new ShowEpisodeItem(this.episodeFile, ef)).ToArray();
                                this.EpisodeSelection.Add(newEpisodeItems,
                                    (i1, i2) =>
                                    {
                                        if (i1 == null) return false;
                                        if (i2 == null) return false;
                                        return string.Equals(i1.ShowTitle, i2.ShowTitle);
                                    });

                                var sel = newEpisodeItems.FirstOrDefault();
                                if (sel != null)
                                {
                                    foreach (var i in this.EpisodeSelection)
                                    {
                                        i.IsSelected = string.Equals(i.ShowTitle, sel.ShowTitle, StringComparison.CurrentCultureIgnoreCase);
                                    }
                                }

                            }

                            this.Status = "";
                        }
                        catch (Exception ex)
                        {
                            this.Status = "ERROR: " + ex.Message;
                        }
                        finally
                        {

                            this.InSearchingMode = false;
                        }

                    });
            }
        }

        public void DoProcessFile()
        {
            if (!this.InSearchingMode)
            {
                this.Status = "Search pending ...";
                this.IsCancelling = false;
                this.InSearchingMode = true;
                this.cancellation = new CancellationTokenSource();

                Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            this.cancellation.Token.ThrowIfCancellationRequested();

                            this.EpisodeSelection.Clear();

                            this.Status = "Extracting season and episode numbers ...";

                            int? seasonIndex;
                            int? episodeIndex;
                            var showName = ProcessFileName(this.episodeFile.FileName, out seasonIndex, out episodeIndex);
                            var originalName = showName;
                            this.episodeFile.OriginalShow = originalName;

                            if (!seasonIndex.HasValue)
                            {
                                throw new Exception("Can't detect season number.");
                            }
                            this.episodeFile.Season = seasonIndex;

                            if (!episodeIndex.HasValue)
                            {
                                throw new Exception("Can't detect episode number.");
                            }
                            this.episodeFile.Episode = episodeIndex;

                            this.cancellation.Token.ThrowIfCancellationRequested();

                            this.Status = "Extracting show name ...";

                            var results = new List<ShowEpisodeItem>();

                            if (!string.IsNullOrWhiteSpace(showName))
                            {
                                // name from fileName
                                results.AddRange(Shows.Current.GetShowEpisode(showName,
                                                                       seasonIndex.GetValueOrDefault(),
                                                                       episodeIndex.GetValueOrDefault(),
                                                                       this.cancellation.Token)
                                                                       .Select(e => new ShowEpisodeItem(episodeFile, e)));
                            }
                            else
                            {
                                var fi = new FileInfo(this.episodeFile.FilePath);
                                if (fi.Directory != null)
                                {
                                    int? seasonD;
                                    int? episodeD;
                                    originalName = ProcessFileName(fi.Directory.Name, out seasonD, out episodeD);
                                    // parent directory as a show name
                                    results.AddRange(Shows.Current.GetShowEpisode(
                                        originalName,
                                        seasonIndex.GetValueOrDefault(),
                                        episodeIndex.GetValueOrDefault(),
                                        this.cancellation.Token)
                                        .Select(e => new ShowEpisodeItem(episodeFile, e)));

                                    //originalName = fi.Directory.Name;

                                    this.cancellation.Token.ThrowIfCancellationRequested();

                                    if (fi.Directory.Parent != null)
                                    {
                                        var originalName2 = ProcessFileName(fi.Directory.Parent.Name, out seasonD, out episodeD);

                                        var c = results.Count;
                                        results.AddRange(Shows.Current.GetShowEpisode(
                                            originalName2,
                                            seasonIndex.GetValueOrDefault(),
                                            episodeIndex.GetValueOrDefault(),
                                            this.cancellation.Token)
                                            .Select(e => new ShowEpisodeItem(episodeFile, e)));

                                        if (c != results.Count)
                                        {
                                            originalName = fi.Directory.Parent.Name;
                                        }
                                    }
                                }
                            }

                            this.cancellation.Token.ThrowIfCancellationRequested();

                            if (results.Count == 0)
                            {
                                throw new Exception("No show found");
                            }

                            if (results.Count == 1)
                            // single result - hopefuly a success
                            {
                                var episodeItem = results[0];

                                if (episodeItem.IsValid)
                                {

                                    this.episodeFile.OriginalShow = originalName;
                                    this.episodeFile.Show = episodeItem.Episode.ShowTitle;
                                    this.episodeFile.Title = episodeItem.Episode.Title;

                                    this.episodeFile.Status = "Ready";
                                    this.Status = "";
                                    this.IsReady = true;
                                    return;
                                }

                            }
                            this.EpisodeSelection.Add(results);
                            results.Last().IsSelected = true;
                            this.Status = "";
                            this.episodeFile.Status = "Please select a show:";
                        }
                        catch (Exception ex)
                        {
                            this.Status = "ERROR: " + ex.Message;

                            throw;
                        }
                        finally
                        {
                            this.episodeFile.InProcessing = false;
                            this.InSearchingMode = false;
                        }
                    });
            }
        }

        private static string ProcessFileName(string fileName, out int? seasonIndex, out int? episodeIndex)
        {
            seasonIndex = null;
            episodeIndex = null;

            var match = episodeIndexRegex.Match(fileName);
            if (match.Success)
            {
                if (match.Groups["s"].Success)
                {
                    int s;
                    if (int.TryParse(match.Groups["s"].Value, out s))
                    {
                        seasonIndex = s;
                    }
                }

                if (match.Groups["e"].Success)
                {
                    int e;
                    if (int.TryParse(match.Groups["e"].Value, out e))
                    {
                        episodeIndex = e;
                    }
                }
                if (match.Index > 0)
                {
                    return fileName.Substring(0, match.Index);
                }
            }
            else
            {
                match = episodeIndexAltRegex.Match(fileName);
                if (match.Success)
                {
                    int number = int.Parse(match.Value);
                    if (number < 100)
                    {
                        seasonIndex = number / 10;
                        episodeIndex = number % 10;
                    }
                    else
                    {
                        seasonIndex = number / 100;
                        episodeIndex = number % 100;
                    }

                    if (match.Index > 0)
                    {
                        return fileName.Substring(0, match.Index);
                    }
                }
            }
            return null;
        }

        internal
            void DoStop()
        {
            this.IsCancelling = true;
            this.Status = "Cancelling ...";
            if (this.cancellation != null)
            {
                this.cancellation.Cancel();
            }

        }

        internal void UseAsIs()
        {
            this.episodeFile.Show = SearchString;
            this.episodeFile.Title = string.Empty;
            this.episodeFile.IsReady = true;
            this.IsReady = true;
            this.episodeFile.Status = "Ready";
        }

        internal void UseSelectedEpisode(bool alwaysUseSelection = false)
        {
            if (!string.IsNullOrWhiteSpace(this.episodeFile.Title))
            {
                this.IsReady = true;
                this.EpisodeSelection.Clear();
                OnPropertyChanged("EpisodeSelection");
                this.episodeFile.Status = "Ready";

                if (alwaysUseSelection)
                {
                    try
                    {
                        Shows.Current.CustomMappings.Add(new CustomMapping(this.episodeFile.OriginalShow, this.episodeFile.Show));
                    }
                    catch { }
                    // it is possible that this selection can affect other files
                    // we need to re-run lookups
                }
            }
        }
    }
}
