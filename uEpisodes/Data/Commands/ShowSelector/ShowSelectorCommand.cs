using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTRider.uEpisodes.Data.Commands.ShowSelector
{
    class ShowSelectorCommand : AppModelCommand
    {
        public override void Execute(object parameter)
        {
            var singleFile = parameter as Data.ShowSelector;
            if (singleFile != null)
            {
                Execute(singleFile);
            }
        }

        public ShowSelectorCommandMode  Mode { get; set; }


        protected virtual void Execute(Data.ShowSelector episodeFile)
        {
            switch (Mode)
            {
                case ShowSelectorCommandMode.Search:
                    episodeFile.DoSearch();
                    break;
                    case ShowSelectorCommandMode.Stop:
                    episodeFile.DoStop();
                    break;
                    case ShowSelectorCommandMode.UseSelected:
                    episodeFile.UseSelectedEpisode();
                    break;
                    case ShowSelectorCommandMode.AlwaysUseSelected:
                    episodeFile.UseSelectedEpisode(true);
                    break;
                case ShowSelectorCommandMode.UseAsIs:
                    episodeFile.UseAsIs();
                    break;
            }
        }
    }

    enum ShowSelectorCommandMode
    {
        None,
        Search,
        Stop,
        UseSelected,
        AlwaysUseSelected,
        UseAsIs
    }
}
