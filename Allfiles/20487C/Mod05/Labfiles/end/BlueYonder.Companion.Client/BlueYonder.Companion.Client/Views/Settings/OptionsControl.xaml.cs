using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BlueYonder.Companion.Client.ViewModels;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BlueYonder.Companion.Client.Views.Settings
{
    public sealed partial class OptionsControl : UserControl
    {
        public OptionsControl()
        {
            this.InitializeComponent();

            this.DataContext = new OptionsViewModel();

            Loaded += Control_Loaded;
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (OptionsViewModel)this.DataContext;
            viewModel.Initialize(null);
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            // Not supported on UWP
            //SettingsPane.Show();
            this.Margin = new Thickness(0, 0, -346, 0);
        }
    }
}
