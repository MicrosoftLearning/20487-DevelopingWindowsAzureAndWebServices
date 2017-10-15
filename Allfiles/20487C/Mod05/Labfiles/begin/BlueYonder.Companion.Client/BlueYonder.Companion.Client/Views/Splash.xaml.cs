using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BlueYonder.Companion.Client.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Splash : Page
    {
        private readonly SplashScreen splash; // Variable to hold the splash screen object

        public Splash(SplashScreen splashScreen)
        {
            this.InitializeComponent();

            splash = splashScreen;

            Window.Current.SizeChanged += Current_SizeChanged;

            PositionImage();
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            // Safely update the extended splash screen image coordinates. This function will be fired in response to snapping, unsnapping, rotation, etc...
            PositionImage();
        }

        void PositionImage()
        {
            // Position the extended splash screen image in the same location as the system splash screen image
            if (splash != null)
            {
                // Retrieve the window coordinates of the splash screen image
                var rect = splash.ImageLocation;
                splashPanel.SetValue(Canvas.LeftProperty, rect.X);
                splashPanel.SetValue(Canvas.TopProperty, rect.Y);
                splashPanel.Width = rect.Width;
                splashPanel.Height = rect.Height + 60;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
    }
}
