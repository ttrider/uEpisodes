namespace TTRider.uEpisodes.Data
{
    class EpisodeFileAction : PropertyStore
    {
        private readonly EpisodeFile file;
        private EpisodeFileActionCommand command;
        private string arguments;
        private bool isEnabled;

        public EpisodeFileAction(EpisodeFile file)
        {
            this.file = file;
            this.file.PropertyChanged += (s, e) => OnPropertyChanged("TargetPath");
        }

        public EpisodeFileActionCommand Command
        {
            get { return this.command; }
            set { SetValue(ref this.command, value, "Command", "TargetPath"); }
        }

        public string Arguments
        {
            get { return this.arguments; }
            set { SetValue(ref this.arguments, value, "Arguments", "TargetPath"); }
        }

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set { SetValue(ref this.isEnabled, value, "IsEnabled", "TargetPath"); }
        }

        public string TargetCommand
        {
            get
            {


                return ((this.Command == EpisodeFileActionCommand.Copy) ? "Copy to "
                    + FileNamePattern.ApplyFilePattern(
                    string.IsNullOrWhiteSpace(this.Arguments) ? file.Model.Settings.CopyToPattern : this.Arguments
                    , this.file.FilePath
                    , this.file.Show
                    , this.file.Season.GetValueOrDefault()
                    , this.file.Episode.GetValueOrDefault()
                    , this.file.Title):
                     "Move to " + FileNamePattern.ApplyFilePattern(
                    string.IsNullOrWhiteSpace(this.Arguments) ? file.Model.Settings.MoveToPattern : this.Arguments
                    , this.file.FilePath
                    , this.file.Show
                    , this.file.Season.GetValueOrDefault()
                    , this.file.Episode.GetValueOrDefault()
                    , this.file.Title));
            }
        }

        public string TargetPath
        {
            get
            {


                return ((this.Command == EpisodeFileActionCommand.Copy) ? FileNamePattern.ApplyFilePattern(
                    string.IsNullOrWhiteSpace(this.Arguments) ? file.Model.Settings.CopyToPattern : this.Arguments
                    , this.file.FilePath
                    , this.file.Show
                    , this.file.Season.GetValueOrDefault()
                    , this.file.Episode.GetValueOrDefault()
                    , this.file.Title) :
                     FileNamePattern.ApplyFilePattern(
                    string.IsNullOrWhiteSpace(this.Arguments) ? file.Model.Settings.MoveToPattern : this.Arguments
                    , this.file.FilePath
                    , this.file.Show
                    , this.file.Season.GetValueOrDefault()
                    , this.file.Episode.GetValueOrDefault()
                    , this.file.Title));
            }
        }
    }
}
