using System;
using System.Collections.Generic;
using System.Linq;
using Cleverscape.UTorrentClient.WebClient;
using System.IO;
using System.Windows.Threading;

namespace TTRider.uEpisodes
{
    public class UTorrentMonitor
    {
        private Dispatcher dispatcher;
        private UTorrentWebClient client;
        private HashSet<string> knownTorrents;

        public event EventHandler<ReadyTorrentsEventArgs> ReadyTorrents;

        public UTorrentMonitor(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.Error = null;
        }

        public Exception Error { get; private set; }

        public bool IsRunning { get { return this.client != null; } }

        private void ReportReadyTorrents()
        {
            if (this.client != null)
            {
                if (this.ReadyTorrents != null)
                {
                    // exclude torrents that was reported before
                    var newTorrents = this.client.Torrents
                                          .Where(
                                              t =>
                                              (t.StatusCategory & TorrentStatusCategory.Completed) ==
                                              TorrentStatusCategory.Completed)
                                          .Where(t => !this.knownTorrents.Contains(t.Hash))
                                          .ToList();

                    if (newTorrents.Count > 0)
                    {
                        foreach (var hash in newTorrents.Select(t => t.Hash))
                        {
                            this.knownTorrents.Add(hash);
                        }

                        this.ReadyTorrents(this, new ReadyTorrentsEventArgs(from newTorrent in newTorrents
                                                   from tf in newTorrent.Files
                                                   select
                                                       new TorrentFileInfo(this,newTorrent, tf)));
                    }
                }
            }
        }

        public void Start(string address, string userName, string password)
        {
            Stop();
            try
            {
                this.knownTorrents = new HashSet<string>();
                this.client = new UTorrentWebClient(address, userName, password, true, this.dispatcher);

                this.client.MinimumTimeBetweenUpdates = TimeSpan.FromSeconds(30);
                this.client.TorrentsUpdated += OnTorrentsUpdated;
                this.client.TorrentFinishedDownloading += OnTorrentFinishedDownloading;
                ReportReadyTorrents();

            }
            catch (Exception ex)
            {
                this.Error = ex;
                Stop();
            }
        }

        void OnTorrentFinishedDownloading(object sender, TorrentEventArgs e)
        {
            ReportReadyTorrents();
        }

        void OnTorrentsUpdated(object sender, EventArgs e)
        {
            ReportReadyTorrents();
        }

        public void Stop()
        {
            if (this.client != null)
            {
                this.client.TorrentsUpdated -= OnTorrentsUpdated;
                this.client.TorrentFinishedDownloading -= OnTorrentFinishedDownloading;
                this.client.Dispose();
                this.client = null;
            }

            this.knownTorrents = null;
        }

        public void RemoveTorrent(Torrent torrent)
        {
            try
            {
                if (this.client != null)
                {
                    this.knownTorrents.Remove(torrent.Hash);
                    this.client.TorrentRemove(torrent);
                }
            }
            catch{}
        }
    }

    public class TorrentFileInfo
    {

        public TorrentFileInfo(UTorrentMonitor monitor, Torrent torrent, TorrentFile torrentFile)
        {
            this.Monitor = monitor;
            this.Torrent = torrent;
            this.TorrentFile = torrentFile;
            this.LocalFileInfo = new FileInfo(Path.Combine(torrent.TargetDirectory, torrentFile.Name));
        }

        public UTorrentMonitor Monitor { get; private set; }
        public Torrent Torrent { get; private set; }
        public TorrentFile TorrentFile { get; private set; }
        public FileInfo LocalFileInfo { get; private set; }

        public void Remove()
        {
            this.Monitor.RemoveTorrent(this.Torrent);
        }

        public void Move(string targetPath)
        {
            throw new NotImplementedException();
        }
    }


    public class ReadyTorrentsEventArgs : EventArgs
    {
        public ReadyTorrentsEventArgs(IEnumerable<TorrentFileInfo> torrentFiles)
        {
            this.TorrentFiles = torrentFiles.ToArray();


        }



        public TorrentFileInfo[] TorrentFiles { get; private set; }
    }

}

