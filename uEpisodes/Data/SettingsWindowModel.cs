using System;
using TTRider.uEpisodes.Properties;

namespace TTRider.uEpisodes.Data
{
    class SettingsWindowModel : EditablePropertyStore
    {
        private string copyToPattern;
        private string moveToPattern;
        private string additionalExtensions;
        private string fileSystemMonitorFolder;
        private bool fileSystemAutoCopy;
        private bool fileSystemAutoMove;
        private bool fileSystamRunMonitor;

        private string uTorrentAddress;
        private bool uTorrentAutoStopSeeding;
        private string uTorrentPassword;
        private string uTorrentUserName;
        private bool uTorrentAutoCopy;
        private bool uTorrentAutoMove;
        private bool uTorrentRunMonitor;

        public SettingsWindowModel()
        {
            this.CopyToPattern = Settings.Default.CopyToPattern;
            this.MoveToPattern = Settings.Default.MoveToPattern;
            this.AdditionalVideoFileExtensions = Settings.Default.AdditionalExtensions;
            this.FileSystemMonitorFolder = Settings.Default.FileSystemMonitorFolder;
            this.FileSystemAutoCopy = Settings.Default.FileSystemAutoCopy;
            this.FileSystemAutoMove = Settings.Default.FileSystemAutoMove;
            this.FileSystemRunMonitor = Settings.Default.FileSystemRunMonitor;

            this.uTorrentAddress= Settings.Default.uTorrentAddress;
            this.uTorrentAutoStopSeeding= Settings.Default.uTorrentAutoStopSeeding;
            this.uTorrentPassword= Settings.Default.uTorrentPassword;
            this.uTorrentUserName= Settings.Default.uTorrentUserName;
            this.uTorrentAutoCopy= Settings.Default.uTorrentAutoCopy;
            this.uTorrentAutoMove= Settings.Default.uTorrentAutoMove;
            this.uTorrentRunMonitor= Settings.Default.uTorrentRunMonitor;

        }

        public string CopyToPattern
        {
            get { return this.copyToPattern; }
            set { SetValue(ref this.copyToPattern, value, "CopyToPattern", "CopyToPatternExample"); }
        }

        public string MoveToPattern
        {
            get { return this.moveToPattern; }
            set { SetValue(ref this.moveToPattern, value, "MoveToPattern", "MoveToPatternExample"); }
        }

        public string FileSystemMonitorFolder
        {
            get { return this.fileSystemMonitorFolder; }
            set { SetValue(ref this.fileSystemMonitorFolder, value, "FileSystemMonitorFolder"); }
        }

        public bool FileSystemAutoCopy
        {
            get { return this.fileSystemAutoCopy; }
            set { SetValue(ref this.fileSystemAutoCopy, value, "FileSystemAutoCopy"); }
        }

        public bool FileSystemAutoMove
        {
            get { return this.fileSystemAutoMove; }
            set { SetValue(ref this.fileSystemAutoMove, value, "FileSystemAutoMove"); }
        }

        public bool FileSystemRunMonitor
        {
            get { return this.fileSystamRunMonitor; }
            set { SetValue(ref this.fileSystamRunMonitor, value, "FileSystemRunMonitor"); }
        }


        public string UTorrentAddress
        {
            get { return this.uTorrentAddress; }
            set { SetValue(ref this.uTorrentAddress, value, "UTorrentAddress"); }
        }

        public string UTorrentPassword
        {
            get { return this.uTorrentPassword; }
            set { SetValue(ref this.uTorrentPassword, value, "UTorrentPassword"); }
        }

        public string UTorrentUserName
        {
            get { return this.uTorrentUserName; }
            set { SetValue(ref this.uTorrentUserName, value, "UTorrentUserName"); }
        }

        public bool UTorrentAutoStopSeeding
        {
            get { return this.uTorrentAutoStopSeeding; }
            set { SetValue(ref this.uTorrentAutoStopSeeding, value, "UTorrentAutoStopSeeding"); }
        }

        public bool UTorrentAutoCopy
        {
            get { return this.uTorrentAutoCopy; }
            set { SetValue(ref this.uTorrentAutoCopy, value, "UTorrentAutoCopy"); }
        }
         
        public bool UTorrentAutoMove
        {
            get { return this.uTorrentAutoMove; }
            set { SetValue(ref this.uTorrentAutoMove, value, "UTorrentAutoMove"); }
        }

        public bool UTorrentRunMonitor
        {
            get { return this.uTorrentRunMonitor; }
            set { SetValue(ref this.uTorrentRunMonitor, value, "UTorrentRunMonitor"); }
        }

        public string CopyToPatternExample
        {
            get
            {
                try
                {
                    return FileNamePattern.ApplyFilePattern(CopyToPattern, "Foo", 1, 2, "Bar");
                }
                catch
                {
                    return "ERROR: Invalid pattern";
                }

            }
        }

        public string MoveToPatternExample
        {
            get
            {
                try
                {
                    return FileNamePattern.ApplyFilePattern(MoveToPattern, "Foo", 1, 2, "Bar");
                }
                catch
                {
                    return "ERROR: Invalid pattern";
                }

            }
        }

        public string AdditionalVideoFileExtensions
        {
            get { return this.additionalExtensions; }
            set { SetValue(ref this.additionalExtensions, string.IsNullOrWhiteSpace(value) ? "" : string.Join("; ", value.Split(new[] { ';', ',', ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)), "AdditionalVideoFileExtensions"); }
        }


        public string SystemVideoFileExtensions
        {
            get { return string.Join("; ", Settings.Default.GetSystemVideoExtensions()); }
        }

        public override void EndEdit()
        {
            base.EndEdit();

            Settings.Default.CopyToPattern = this.CopyToPattern;
            Settings.Default.MoveToPattern = this.MoveToPattern;
            Settings.Default.AdditionalExtensions = this.AdditionalVideoFileExtensions;

            Settings.Default.FileSystemMonitorFolder = this.FileSystemMonitorFolder;
            Settings.Default.FileSystemAutoCopy = this.FileSystemAutoCopy;
            Settings.Default.FileSystemAutoMove = this.FileSystemAutoMove;
            Settings.Default.FileSystemRunMonitor = this.FileSystemRunMonitor;

            Settings.Default.uTorrentAddress=this.uTorrentAddress;
            Settings.Default.uTorrentAutoStopSeeding = this.uTorrentAutoStopSeeding; 
            Settings.Default.uTorrentPassword =this.uTorrentPassword;
            Settings.Default.uTorrentUserName = this.uTorrentUserName; 
            Settings.Default.uTorrentAutoCopy=this.uTorrentAutoCopy;
            Settings.Default.uTorrentAutoMove=this.uTorrentAutoMove ;
            Settings.Default.uTorrentRunMonitor=this.uTorrentRunMonitor;


            Settings.Default.Save();
        }
    }
}
