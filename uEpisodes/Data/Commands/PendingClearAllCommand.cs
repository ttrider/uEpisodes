using System.Linq;

namespace TTRider.uEpisodes.Data.Commands
{
    class PendingClearAllCommand : AppModelCommand
    {
        public override void Execute(object parameter)
        {
            foreach (var a in Model.Files.SelectMany(file => file.Actions))
            {
                a.IsEnabled = false;
            }
        }
    }
}