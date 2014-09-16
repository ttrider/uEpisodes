using System.Windows;
using TTRider.uEpisodes.Data;
using System.Collections.Generic;

namespace TTRider.uEpisodes
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileDrop"))
            {
                AppModel.Current.OpenFileSystemItems(e.Data.GetData("FileDrop") as IEnumerable<string>);
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
