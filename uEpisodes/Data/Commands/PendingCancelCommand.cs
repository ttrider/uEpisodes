using System.Linq;

namespace TTRider.uEpisodes.Data.Commands
{
    class PendingCancelCommand : EpisodeAppModelCommand
    {
        protected override void Execute(EpisodeFile episodeFile)
        {
            episodeFile.Cancel();
            Model.Files.Remove(episodeFile);
        }
    }
}