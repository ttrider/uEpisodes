using System.Linq;

namespace TTRider.uEpisodes.Data.Commands
{
    class PendingRetryAllCommand : AppModelCommand
    {
        public override void Execute(object parameter)
        {
            foreach (var file in Model.Files.Where(f => !f.InProcessing))
            {
                file.ShowSelector.DoProcessFile();
            }
        }
    }
}