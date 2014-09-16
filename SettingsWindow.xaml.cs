using System;
using System.Diagnostics;
using System.Windows;
using Cleverscape.UTorrentClient.WebClient;
using TTRider.uEpisodes.Data;
using WPFFolderBrowser;

namespace TTRider.uEpisodes
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            ((SettingsWindowModel)FindResource("model")).EndEdit();
            this.DialogResult = true;
        }

        private void StartStopMonitoring(object sender, RoutedEventArgs e)
        {
            var model = ((SettingsWindowModel)FindResource("model"));
            model.FileSystemRunMonitor = !model.FileSystemRunMonitor;

        }

        private void UStartStopMonitoring(object sender, RoutedEventArgs e)
        {
            var model = ((SettingsWindowModel)FindResource("model"));
            model.UTorrentRunMonitor = !model.UTorrentRunMonitor;

        }

        private void BrowseForFolder(object sender, RoutedEventArgs e)
        {
            var model = ((SettingsWindowModel)FindResource("model"));
            
            var dlg = new WPFFolderBrowserDialog("Select Folder")
            {
                AddToMruList = true,
                DereferenceLinks = true,
                ShowPlacesList = true,
                InitialDirectory = model.FileSystemMonitorFolder
            };

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                model.FileSystemMonitorFolder = dlg.FileName;
                
            }
        }

        private void CheckConnection(object sender, RoutedEventArgs e)
        {
            try
            {
                var model = ((SettingsWindowModel)FindResource("model"));

                var client = new UTorrentWebClient(model.UTorrentAddress, model.UTorrentUserName, model.UTorrentPassword);
                var test = client.Settings.Count;
                Trace.WriteLine(string.Format("Check connection: success - loaded {0} settings",test));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var model = ((SettingsWindowModel)FindResource("model"));
            model.UTorrentPassword = PasswordControl.Password;
        }
    }
}
