using System.Linq;

namespace TTRider.uEpisodes.Data.Commands
{
    class RunningRetryCommand : EpisodeAppModelCommand
    {
        protected override void Execute(EpisodeFile episodeFile)
        {
            if (Model.RunningFiles.Contains(episodeFile) && (episodeFile.RetryTimer != null))
            {
                episodeFile.Cancel();
                Model.RunningFiles.Remove(episodeFile);
            }
        }

        protected override bool CanExecute(EpisodeFile episodeFile)
        {
            return (Model.RunningFiles.Contains(episodeFile) && (episodeFile.RetryTimer != null));
        }
    }
}