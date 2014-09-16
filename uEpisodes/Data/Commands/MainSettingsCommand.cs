using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace TTRider.uEpisodes.Data.Commands
{
    internal class MainSettingsCommand : AppModelCommand
    {
        public override void Execute(object parameter)
        {
            new SettingsWindow {Owner = System.Windows.Application.Current.MainWindow}.ShowDialog();
        }
    }
}
