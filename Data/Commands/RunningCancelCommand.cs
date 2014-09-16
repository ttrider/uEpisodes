using System.Linq;

namespace TTRider.uEpisodes.Data.Commands
{
    class RunningCancelCommand : EpisodeAppModelCommand
    {
        protected override void Execute(EpisodeFile episodeFile)
        {
            //episodeFile.Cancel();
            //Model.Files.Remove(episodeFile);
        }
    }
}