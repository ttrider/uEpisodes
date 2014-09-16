using System.Linq;

namespace TTRider.uEpisodes.Data.Commands
{
    class PendingPerformAllCommand : AppModelCommand
    {
        public override void Execute(object parameter)
        {
            foreach (var file in Model.Files.Where(f => f.Actions.Any(a => a.IsEnabled)).ToArray())
            {
                Model.Files.Remove(file);
                Model.RunningFiles.Add(file);
            }
        }
    }
}