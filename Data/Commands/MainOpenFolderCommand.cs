using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPFFolderBrowser;

namespace TTRider.uEpisodes.Data.Commands
{
    internal class MainOpenFolderCommand : AppModelCommand
    {
        public override void Execute(object parameter)
        {
            var dlg = new WPFFolderBrowserDialog("Select Folder")
            {
                AddToMruList = true,
                DereferenceLinks = true,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                Model.OpenFileSystemItems(dlg.FileName);
            }
        }
    }
}
