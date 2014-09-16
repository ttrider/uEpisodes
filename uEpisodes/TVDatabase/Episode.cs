namespace TTRider.uEpisodes.TVDatabase
{
    class Episode : DataItem
    {
        public Episode(string showTitle, int? season, int? episode, string title, bool isValid = true)
            : this(showTitle, season.HasValue ? season.ToString() : null, episode.HasValue ? episode.ToString() : null, title, isValid)
        {
            
        }

        public Episode(string showTitle, string season, string episode, string title, bool isValid = true)
        {
            this.ShowTitle = showTitle;
            int value;
            if (int.TryParse(season, out value))
            {
                SeasonIndex = value;
            }

            if (int.TryParse(episode, out value))
            {
                EpisodeIndex = value;
            }
            Title = title;
            IsValid = isValid;
        }

        public string ShowTitle { get; private set; }
        public int SeasonIndex { get; private set; }
        public int EpisodeIndex { get; private set; }
        public string Title { get; private set; }

        public bool IsValid { get; private set; }
    }
}
