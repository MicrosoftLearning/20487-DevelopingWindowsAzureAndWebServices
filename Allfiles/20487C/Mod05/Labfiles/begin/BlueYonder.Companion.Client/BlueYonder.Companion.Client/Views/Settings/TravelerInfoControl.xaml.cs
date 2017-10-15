using BlueYonder.Companion.Client.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BlueYonder.Companion.Client.Views.Settings
{
    public sealed partial class TravelerInfoControl : UserControl
    {
        public TravelerInfoControl()
        {
            this.InitializeComponent();

            this.DataContext = new TravelerInfoViewModel();
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            // Not supported on UWP
            //SettingsPane.Show();
            this.Margin = new Thickness(0, 0, -346, 0);
        }
    }
}
