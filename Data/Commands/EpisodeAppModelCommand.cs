using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace TTRider.uEpisodes.Data.Commands
{
    abstract class EpisodeAppModelCommand : AppModelCommand
    {
        public override void Execute(object parameter)
        {
            var singleFile = parameter as EpisodeFile;
            if (singleFile != null)
            {
                Execute(singleFile);
            }
        }

        public override bool CanExecute(object parameter)
        {
            var singleFile = parameter as EpisodeFile;
            if (singleFile != null)
            {
                return CanExecute(singleFile);
            }
            return base.CanExecute(parameter);
        }

        protected abstract void Execute(EpisodeFile episodeFile);

        protected virtual bool CanExecute(EpisodeFile episodeFile)
        {
            return true;
        }
    }
}
