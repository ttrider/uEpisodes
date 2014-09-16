using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TTRider.uEpisodes.Data;
using TTRider.uEpisodes.TVDatabase;

namespace TTRider.uEpisodes
{
    /// <summary>
    /// Interaction logic for ShowSelectorControl.xaml
    /// </summary>
    public partial class ShowSelectorControl : UserControl
    {
        public ShowSelectorControl()
        {
            InitializeComponent();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchButton.DoClick();
                
            }
        }
    }
}
