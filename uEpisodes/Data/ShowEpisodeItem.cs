using TTRider.uEpisodes.TVDatabase;

namespace TTRider.uEpisodes.Data
{
    class ShowEpisodeItem : PropertyStore
    {
        private readonly EpisodeFile file;
        private readonly Episode episode;
        private bool isSelected;

        public ShowEpisodeItem(EpisodeFile file, Episode episode)
        {
            this.file = file;
            this.episode = episode;
        }

        public bool IsValid
        {
            get { return !string.IsNullOrWhiteSpace(episode.Title); }
        }

        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if ((value && !string.IsNullOrWhiteSpace(episode.Title)) || !value)
                {
                    SetValue(ref isSelected, value, (vOld, vNew) =>
                    {
                        if (vNew)
                        {
                            this.file.Show = episode.ShowTitle;
                            this.file.Title = episode.Title;
                        }
                    }, "IsSelected");
                }
            }
        }

        public string File
        {
            get { return file.FilePath; }
        }

        public Episode Episode
        {
            get { return episode; }
        }

        public string ShowTitle { get { return episode.ShowTitle + " - " + (string.IsNullOrWhiteSpace(episode.Title) ? "<invalid season/episode index combination>" : "'" + episode.Title + "'"); } }

    }
}
