using System.Linq;

namespace TTRider.uEpisodes.Data.Commands
{
    class PendingPerformCommand : EpisodeAppModelCommand
    {
        protected override void Execute(EpisodeFile episodeFile)
        {
            if (episodeFile.CanPerform)
            {
                Model.Files.Remove(episodeFile);
                Model.RunningFiles.Add(episodeFile);
            }
        }
    }
}