using System.Linq;

namespace TTRider.uEpisodes.Data.Commands
{
    class PendingRetryCommand : EpisodeAppModelCommand
    {
        protected override void Execute(EpisodeFile episodeFile)
        {
            if (Model.Files.Contains(episodeFile) && !episodeFile.InProcessing)
            {
                episodeFile.ShowSelector.DoProcessFile();
            }
        }
    }
}