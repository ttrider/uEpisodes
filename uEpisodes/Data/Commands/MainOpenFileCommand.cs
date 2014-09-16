using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using TTRider.uEpisodes.Properties;

namespace TTRider.uEpisodes.Data.Commands
{
    internal class MainOpenFileCommand : AppModelCommand
    {
        public override void Execute(object parameter)
        {
            

            var dlg = new OpenFileDialog
            {
                DefaultExt = ".avi",
                Filter =
                    "Video Files|" + string.Join(";", Settings.Default.GetVideoExtensions().Select(ee => "*" + ee)) + "|All Files|*.*",
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = true,
                Multiselect = true,
                Title = "Select video files"
            };

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                Model.OpenFileSystemItems(dlg.FileNames);
            }
        }
    }

}
