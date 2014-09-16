using System.Linq;

namespace TTRider.uEpisodes.Data.Commands
{
    class PendingCopyAllCommand : AppModelCommand
    {
        public override void Execute(object parameter)
        {
            foreach (
                var a in
                    Model.Files.SelectMany(file => file.Actions)
                         .Where(a => a.Command == EpisodeFileActionCommand.Copy))
            {
                a.IsEnabled = true;
            }
        }
    }
}