using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TTRider.uEpisodes.TVDatabase;

namespace TTRider.uEpisodes.Data
{
    class EpisodeFile : PropertyStore
    {

        string filePath;
        string fileName;
        string show;
        int? season;
        private string originalShow;
        int? episode;
        string title;
        string status;
        
        private bool isReady;
        private bool inProcessing;

        
       
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

        public EpisodeFile(AppModel owner, string filePath, HashSet<string> copySet = null, HashSet<string> moveSet = null)
        {
            this.Model = owner;
            this.Actions = new DispatchedCollection<EpisodeFileAction>();
            this.Errors = new DispatchedCollection<string>();
            

            if (copySet != null)
            {
                if (copySet.Count > 0)
                {
                    foreach (var fa in
                        copySet.Select(
                            s =>
                            new EpisodeFileAction(this)
                                {
                                    Arguments = s,
                                    Command = EpisodeFileActionCommand.Copy,
                                    IsEnabled = true
                                }))
                    {
                        this.Actions.Add(fa);
                    }
                }
                else
                {
                    this.Actions.Add(new EpisodeFileAction(this)
                    {
                        Command = EpisodeFileActionCommand.Copy,
                        IsEnabled = true
                    });
                }
            }

            if (moveSet != null)
            {
                if (moveSet.Count > 0)
                {
                    foreach (var fa in
                        moveSet.Select(
                            s =>
                            new EpisodeFileAction(this)
                            {
                                Arguments = s,
                                Command = EpisodeFileActionCommand.Move,
                                IsEnabled = true
                            }))
                    {
                        this.Actions.Add(fa);
                    }
                }
                else
                {
                    this.Actions.Add(new EpisodeFileAction(this)
                    {
                        Command = EpisodeFileActionCommand.Move,
                        IsEnabled = true
                    });
                }
            }

            if (Actions.All(f => f.Command != EpisodeFileActionCommand.Copy))
            {
                Actions.Add(new EpisodeFileAction(this) { Command = EpisodeFileActionCommand.Copy });
            }

            if (Actions.All(f => f.Command != EpisodeFileActionCommand.Move))
            {
                Actions.Add(new EpisodeFileAction(this) { Command = EpisodeFileActionCommand.Move });
            }

            this.inProcessing = true;
            this.FilePath = filePath;
            this.FileName = Path.GetFileName(this.FilePath);
            this.Status = "";
            
            this.ShowSelector = new ShowSelector(this);
            this.ShowSelector.DoProcessFile();
        }



        #region Properties

        public AppModel Model { get; set; }

        public bool IsReady
        {
            get { return this.isReady; }
            set
            {
                SetValue(ref this.isReady, value, "IsReady", "CanPerform");
            }
        }

        public bool InProcessing
        {
            get { return this.inProcessing; }
            set
            {
                SetValue(ref this.inProcessing, value, "InProcessing", "CanPerform");
            }
        }

        public bool CanPerform
        {
            get { return this.IsReady && !this.InProcessing && this.Actions.Any(f => f.IsEnabled); }
        }


        public string FileName
        {
            get { return this.fileName; }
            private set
            {
                SetValue(ref fileName, value, "FileName");
            }
        }

        public string FilePath
        {
            get { return this.filePath; }
            private set
            {
                SetValue(ref filePath, value, "FilePath");
            }
        }

        public string Show
        {
            get { return this.show; }
            set
            {
                SetValue(ref show, value, "Show");
            }
        }

        public string OriginalShow
        {
            get { return this.originalShow; }
            set
            {
                SetValue(ref originalShow, value, "OriginalShow");
            }
        }

        public int? Season
        {
            get { return this.season; }
            set
            {
                SetValue(ref this.season, value, "Season");
            }
        }

        public int? Episode
        {
            get { return this.episode; }
            set
            {
                SetValue(ref this.episode, value, "Episode");
            }
        }

        public string Title
        {
            get { return this.title; }
            set
            {
                SetValue(ref this.title, value, "Title");
            }
        }

        public string Status
        {
            get { return this.status; }
            set
            {
                SetValue(ref this.status, value, "Status");
            }
        }

        public DispatchedCollection<string> Errors { get; private set; }

        public DispatchedCollection<EpisodeFileAction> Actions { get; private set; }

        public DispatcherTimer RetryTimer
        {
            get;
            set;
        }

        #endregion Properties


       

        internal void Cancel()
        {
            this.cancellation.Cancel();

            if (RetryTimer != null)
            {
                RetryTimer.Stop();

            }


        }



        public ShowSelector ShowSelector { get; private set; }
    }
}
