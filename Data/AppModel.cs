using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TTRider.uEpisodes.Properties;

namespace TTRider.uEpisodes.Data
{
    internal class AppModel : PropertyStore
    {
        private readonly Lazy<AppModelTraceListener> listener;
        private FileSystemWatcher watcher;
        private UTorrentMonitor uMonitor;


        public static AppModel Current
        {
            get { return Application.Current.FindResource("appData") as AppModel; }
        }

        public AppModel()
        {
            this.listener = new Lazy<AppModelTraceListener>(() => Trace.Listeners.OfType<AppModelTraceListener>().FirstOrDefault());

            this.uMonitor = new UTorrentMonitor(Application.Current.Dispatcher);
            this.uMonitor.ReadyTorrents += OnTorrentReady;

            this.WorkQueue = new BlockingCollection<EpisodeFile>();
            this.RunningFiles = new ObservableCollection<EpisodeFile>();
            this.Files = new ObservableCollection<EpisodeFile>();
            this.CompletedFiles = new ObservableCollection<EpisodeFile>();

            this.Files.CollectionChanged += (s, e) =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Remove:
                            foreach (var item in e.OldItems.Cast<EpisodeFile>())
                            {
                                item.Cancel();
                            }
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            foreach (var item in this.Files)
                            {
                                item.Cancel();
                            }
                            break;
                    }

                    OnPropertyChanged("IsFileSetEmpty", "Status");
                };

            this.RunningFiles.CollectionChanged += (s, e) =>
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        foreach (var item in e.NewItems.Cast<EpisodeFile>())
                        {
                            item.Status = "Waiting ...";
                            this.WorkQueue.Add(item);
                        }
                    }
                };

            Task.Factory.StartNew(() =>
                {
                    foreach (var item in WorkQueue.GetConsumingEnumerable())
                    {
                        var file = item;

                        // do the processing
                        file.Status = "Processing ...";

                        var errors = new List<string>();


                        // first do copy
                        foreach (
                            var action in
                                file.Actions.Where(a => a.Command == EpisodeFileActionCommand.Copy && a.IsEnabled))
                        {
                            try
                            {
                                item.Status = action.TargetCommand;
                                var dir = Path.GetDirectoryName(action.TargetPath);
                                if ((dir != null) && (!Directory.Exists(dir)))
                                {
                                    Directory.CreateDirectory(dir);
                                }

                                var fo = new InteropSHFileOperation()
                                {
                                    pFrom = item.FilePath,
                                    pTo = action.TargetPath,
                                    wFunc = InteropSHFileOperation.FO_Func.FO_COPY,
                                    fFlags = new InteropSHFileOperation.FILEOP_FLAGS()
                                    {
                                        FOF_RENAMEONCOLLISION = true
                                    }
                                };
                                fo.Execute();

                                //File.Copy(item.FilePath, action.TargetPath, false);
                                action.IsEnabled = false;
                            }
                            catch (Exception ex)
                            {
                                errors.Add(ex.Message);
                            }
                        }

                        var renameAction =
                            file.Actions.FirstOrDefault(a => a.Command == EpisodeFileActionCommand.Move && a.IsEnabled);


                        if (errors.Count == 0 && renameAction != null)
                        {
                            try
                            {
                                file.Status = renameAction.TargetCommand;
                                var dir = Path.GetDirectoryName(renameAction.TargetPath);
                                if ((dir != null) && (!Directory.Exists(dir)))
                                {
                                    Directory.CreateDirectory(dir);
                                }

                                var fo = new InteropSHFileOperation()
                                {
                                    pFrom = item.FilePath,
                                    pTo = renameAction.TargetPath,
                                    wFunc = InteropSHFileOperation.FO_Func.FO_MOVE,
                                    fFlags = new InteropSHFileOperation.FILEOP_FLAGS()
                                    {
                                        FOF_RENAMEONCOLLISION = true
                                    }
                                };
                                fo.Execute();
                                renameAction.IsEnabled = false;
                            }
                            catch (Exception ex)
                            {
                                errors.Add(ex.Message);
                            }
                        }

                        if (errors.Count == 0)
                        {
                            file.Status = "Done";
                                try
                                {

                            //detect if this directory contains any more video files or files bigger then 1MB
                            // if not, delete it
                                    var dir = Path.GetDirectoryName(file.FilePath);
                            if (!Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories).Select(fl => new FileInfo(fl)).Any(fl =>
                                ((fl.Length > 1024 * 1024) || Settings.GetVideoExtensions().Any(ex => string.Equals(fl.Extension, ex, StringComparison.CurrentCultureIgnoreCase)))
                            ))
                            {
                                // delete directory
                                    Directory.Delete(dir, true);
                            }
                                }
                                catch
                                {
                                }


                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    file.Errors.Clear();
                                    RunningFiles.Remove(file);
                                    CompletedFiles.Add(file);
                                }));
                        }
                        else
                        {

                            file.Status = "Waiting for retry";

                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    file.Errors.Clear();
                                    foreach (var err in errors)
                                    {
                                        file.Errors.Add(err);
                                    }

                                }));

                            file.RetryTimer = new DispatcherTimer(TimeSpan.FromMinutes(5), DispatcherPriority.Normal,
                                                                  (s, e) =>
                                                                  {
                                                                      file.Errors.Clear();


                                                                      file.RetryTimer.Stop();

                                                                      file.Status = "Pending ...";
                                                                      this.WorkQueue.Add(file);

                                                                  },
                                                                  Application.Current.Dispatcher);

                        }
                    }
                });

            this.RunWatcher();
            //this.Watcher.EnableRaisingEvents = this.Settings.RunMonitor;
            this.Settings.SettingsSaving += Settings_SettingsSaving;
        }

        void OnTorrentReady(object sender, ReadyTorrentsEventArgs e)
        {
            HashSet<string> copySet = null;
            if (!string.IsNullOrWhiteSpace(this.Settings.CopyToPattern) && this.Settings.uTorrentAutoCopy)
            {
                copySet = new HashSet<string>();
                copySet.Add(this.Settings.CopyToPattern);
            }

            HashSet<string> moveSet = null;
            if (!string.IsNullOrWhiteSpace(this.Settings.MoveToPattern) && Settings.uTorrentAutoMove)
            {
                moveSet = new HashSet<string>();
                moveSet.Add(Settings.Default.MoveToPattern);
            }

            var scheduleForStopSeeding = this.Settings.uTorrentAutoMove && this.Settings.uTorrentAutoStopSeeding;

            foreach (var tf in e.TorrentFiles)
            {
                try
                {
                    this.OpenFileSystemItems(tf.LocalFileInfo.FullName, copySet, moveSet);

                    if (scheduleForStopSeeding)
                    {
                        tf.Remove();
                    }
                }
                catch (Exception ex)
                {
                    Trace.Write(ex.Message);
                }
            }
        }

        void Settings_SettingsSaving(object sender, CancelEventArgs e)
        {
            RunWatcher();


        }

        FileSystemWatcher RunWatcher()
        {
            this.uMonitor.Stop();
            if (this.Settings.uTorrentRunMonitor)
            {
                this.uMonitor.Start(this.Settings.uTorrentAddress, this.Settings.uTorrentUserName, this.Settings.uTorrentPassword);
            }


            if (this.watcher != null)
            {
                this.watcher.EnableRaisingEvents = false;
                this.watcher.Dispose();
                this.watcher = null;
            }

            this.watcher = new FileSystemWatcher();
            this.watcher.IncludeSubdirectories = true;
            this.watcher.NotifyFilter = NotifyFilters.FileName;
            this.watcher.Created += (s, e) =>
            {
                HashSet<string> copySet = null;
                if (!string.IsNullOrWhiteSpace(Settings.Default.CopyToPattern) && Settings.FileSystemAutoCopy)
                {
                    copySet = new HashSet<string>();
                    copySet.Add(Settings.Default.CopyToPattern);
                }

                HashSet<string> moveSet = null;
                if (!string.IsNullOrWhiteSpace(Settings.Default.CopyToPattern) && Settings.FileSystemAutoCopy)
                {
                    moveSet = new HashSet<string>();
                    moveSet.Add(Settings.Default.MoveToPattern);
                }

                this.OpenFileSystemItems(e.FullPath, copySet, moveSet);
            };
            if (Directory.Exists(Settings.Default.FileSystemMonitorFolder))
            {
                Trace.WriteLine("Monitor folder: " + this.watcher.Path);
                this.watcher.Path = Settings.Default.FileSystemMonitorFolder;

                try
                {
                    this.watcher.EnableRaisingEvents = this.Settings.FileSystemRunMonitor;

                }
                catch (Exception exception)
                {

                    Trace.WriteLine(exception.Message);
                }

            }
            return this.watcher;
        }

        public string Version
        {
            get { return Assembly.GetEntryAssembly().GetName().Version.ToString(); }
        }

        public string ApplicationName
        {
            get { return "μEpisodes"; }
        }

        public Settings Settings
        {
            get { return Settings.Default; }
        }

        public ObservableCollection<EpisodeFile> Files { get; private set; }

        public ObservableCollection<EpisodeFile> RunningFiles { get; private set; }

        public ObservableCollection<EpisodeFile> CompletedFiles { get; private set; }

        public BlockingCollection<EpisodeFile> WorkQueue { get; private set; }

        public ObservableCollection<AppModelTraceItem> TraceItems
        {
            get
            {
                if (this.listener.Value == null)
                {
                    return null;
                }
                return this.listener.Value.LogItems;
            }
        }


        public ProcessingQueue ProcessingQueue
        {
            get { return ProcessingQueue.Current; }
        }


        internal EpisodeFile AddFile(string item, HashSet<string> copySet = null, HashSet<string> moveSet = null)
        {
            //only add if this file doesn't exists yet.
            if (this.Files.Any(fi => string.Equals(fi.FilePath, item, StringComparison.OrdinalIgnoreCase)))
            {
                Trace.WriteLine("File '{0}' is already in processing", item);
                return null;
            }

            var file = new EpisodeFile(this, item, copySet, moveSet);

            if (copySet != null || moveSet != null)
            {
                // make auto-process on
                file.PropertyChanged += AutoProcessFile;
            }

            if (Application.Current.Dispatcher.CheckAccess())
            {
                this.Files.Add(file);
            }
            else
            {

                Application.Current.Dispatcher.BeginInvoke(new Action(() => this.Files.Add(file)));
            }

            return file;

        }

        private void AutoProcessFile(object source, PropertyChangedEventArgs e)
        {
            var file = (EpisodeFile)source;

            if ((e.PropertyName == "IsReady") && (file.IsReady))
            {
                file.PropertyChanged -= AutoProcessFile;
                this.Files.Remove(file);
                this.RunningFiles.Add(file);
            }

            if ((e.PropertyName == "InProcessing") && (!file.InProcessing))
            {
                file.PropertyChanged -= AutoProcessFile;
            }
        }


        public void OpenFileSystemItems(string fileSystemItem, HashSet<string> copySet = null,
                                        HashSet<string> moveSet = null)
        {
            if (!string.IsNullOrWhiteSpace(fileSystemItem))
            {
                OpenFileSystemItems(new[] { fileSystemItem }, copySet, moveSet);
            }
        }

        public void OpenFileSystemItems(IEnumerable<string> fileSystemItems, HashSet<string> copySet = null,
                                        HashSet<string> moveSet = null)
        {
            if (fileSystemItems != null)
            {
                var exts = Settings.Default.GetVideoExtensions();

                Task.Factory.StartNew(() =>
                    {
                        foreach (var file in fileSystemItems
                            .SelectMany(
                                fs => ((File.GetAttributes(fs) & FileAttributes.Directory) == FileAttributes.Directory)
                                          ? new DirectoryInfo(fs).GetFiles("*.*", SearchOption.AllDirectories)
                                          : new[] { new FileInfo(fs) })
                            .Where(fl => exts.Contains(fl.Extension))
                            )
                        {
                            Trace.TraceInformation("Adding file '{0}'", file.FullName);
                            this.AddFile(file.FullName, copySet, moveSet);
                        }
                    });
            }


        }


    }

    public class InteropSHFileOperation
    {
        public enum FO_Func : uint
        {
            FO_MOVE = 0x0001,
            FO_COPY = 0x0002,
            FO_DELETE = 0x0003,
            FO_RENAME = 0x0004,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 2)]
        struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public FO_Func wFunc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pTo;
            public ushort fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszProgressTitle;

        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHFileOperation([In, Out] ref SHFILEOPSTRUCT lpFileOp);

        private SHFILEOPSTRUCT _ShFile;
        public FILEOP_FLAGS fFlags;

        public IntPtr hwnd
        {
            set
            {
                this._ShFile.hwnd = value;
            }
        }
        public FO_Func wFunc
        {
            set
            {
                this._ShFile.wFunc = value;
            }
        }

        public string pFrom
        {
            set
            {
                this._ShFile.pFrom = value + '\0' + '\0';
            }
        }
        public string pTo
        {
            set
            {
                this._ShFile.pTo = value + '\0' + '\0';
            }
        }

        public bool fAnyOperationsAborted
        {
            set
            {
                this._ShFile.fAnyOperationsAborted = value;
            }
        }
        public IntPtr hNameMappings
        {
            set
            {
                this._ShFile.hNameMappings = value;
            }
        }
        public string lpszProgressTitle
        {
            set
            {
                this._ShFile.lpszProgressTitle = value + '\0';
            }
        }

        public InteropSHFileOperation()
        {

            this.fFlags = new FILEOP_FLAGS();
            this._ShFile = new SHFILEOPSTRUCT();
            this._ShFile.hwnd = IntPtr.Zero;
            this._ShFile.wFunc = FO_Func.FO_COPY;
            this._ShFile.pFrom = "";
            this._ShFile.pTo = "";
            this._ShFile.fAnyOperationsAborted = false;
            this._ShFile.hNameMappings = IntPtr.Zero;
            this._ShFile.lpszProgressTitle = "";

        }

        public bool Execute()
        {
            this._ShFile.fFlags = this.fFlags.Flag;
            return SHFileOperation(ref this._ShFile) == 0;//true if no errors
        }

        public class FILEOP_FLAGS
        {
            [Flags]
            private enum FILEOP_FLAGS_ENUM : ushort
            {
                FOF_MULTIDESTFILES = 0x0001,
                FOF_CONFIRMMOUSE = 0x0002,
                FOF_SILENT = 0x0004,  // don't create progress/report
                FOF_RENAMEONCOLLISION = 0x0008,
                FOF_NOCONFIRMATION = 0x0010,  // Don't prompt the user.
                FOF_WANTMAPPINGHANDLE = 0x0020,  // Fill in SHFILEOPSTRUCT.hNameMappings
                // Must be freed using SHFreeNameMappings
                FOF_ALLOWUNDO = 0x0040,
                FOF_FILESONLY = 0x0080,  // on *.*, do only files
                FOF_SIMPLEPROGRESS = 0x0100,  // means don't show names of files
                FOF_NOCONFIRMMKDIR = 0x0200,  // don't confirm making any needed dirs
                FOF_NOERRORUI = 0x0400,  // don't put up error UI
                FOF_NOCOPYSECURITYATTRIBS = 0x0800,  // dont copy NT file Security Attributes
                FOF_NORECURSION = 0x1000,  // don't recurse into directories.
                FOF_NO_CONNECTED_ELEMENTS = 0x2000,  // don't operate on connected elements.
                FOF_WANTNUKEWARNING = 0x4000,  // during delete operation, warn if nuking instead of recycling (partially overrides FOF_NOCONFIRMATION)
                FOF_NORECURSEREPARSE = 0x8000,  // treat reparse points as objects, not containers
            }

            public bool FOF_MULTIDESTFILES = false;
            public bool FOF_CONFIRMMOUSE = false;
            public bool FOF_SILENT = false;
            public bool FOF_RENAMEONCOLLISION = false;
            public bool FOF_NOCONFIRMATION = false;
            public bool FOF_WANTMAPPINGHANDLE = false;
            public bool FOF_ALLOWUNDO = false;
            public bool FOF_FILESONLY = false;
            public bool FOF_SIMPLEPROGRESS = false;
            public bool FOF_NOCONFIRMMKDIR = false;
            public bool FOF_NOERRORUI = false;
            public bool FOF_NOCOPYSECURITYATTRIBS = false;
            public bool FOF_NORECURSION = false;
            public bool FOF_NO_CONNECTED_ELEMENTS = false;
            public bool FOF_WANTNUKEWARNING = false;
            public bool FOF_NORECURSEREPARSE = false;

            public ushort Flag
            {
                get
                {
                    ushort ReturnValue = 0;

                    if (this.FOF_MULTIDESTFILES)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_MULTIDESTFILES;
                    if (this.FOF_CONFIRMMOUSE)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_CONFIRMMOUSE;
                    if (this.FOF_SILENT)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_SILENT;
                    if (this.FOF_RENAMEONCOLLISION)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_RENAMEONCOLLISION;
                    if (this.FOF_NOCONFIRMATION)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_NOCONFIRMATION;
                    if (this.FOF_WANTMAPPINGHANDLE)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_WANTMAPPINGHANDLE;
                    if (this.FOF_ALLOWUNDO)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_ALLOWUNDO;
                    if (this.FOF_FILESONLY)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_FILESONLY;
                    if (this.FOF_SIMPLEPROGRESS)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_SIMPLEPROGRESS;
                    if (this.FOF_NOCONFIRMMKDIR)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_NOCONFIRMMKDIR;
                    if (this.FOF_NOERRORUI)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_NOERRORUI;
                    if (this.FOF_NOCOPYSECURITYATTRIBS)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_NOCOPYSECURITYATTRIBS;
                    if (this.FOF_NORECURSION)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_NORECURSION;
                    if (this.FOF_NO_CONNECTED_ELEMENTS)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_NO_CONNECTED_ELEMENTS;
                    if (this.FOF_WANTNUKEWARNING)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_WANTNUKEWARNING;
                    if (this.FOF_NORECURSEREPARSE)
                        ReturnValue |= (ushort)FILEOP_FLAGS_ENUM.FOF_NORECURSEREPARSE;

                    return ReturnValue;
                }
            }
        }

    }
}
