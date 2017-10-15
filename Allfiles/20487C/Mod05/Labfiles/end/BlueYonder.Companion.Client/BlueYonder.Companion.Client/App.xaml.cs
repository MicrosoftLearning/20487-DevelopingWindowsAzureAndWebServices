using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BlueYonder.Companion.Client.Helpers;
using System.Threading.Tasks;
using BlueYonder.Companion.Client.Common;
//using BlueYonder.Companion.Client.Common;
using BlueYonder.Companion.Client.Views;

namespace BlueYonder.Companion.Client
{
    sealed partial class App : Application
    {
        private Frame _rootFrame;

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            base.OnWindowCreated(args);
            //SearchPane searchPane = SearchPane.GetForCurrentView();
            //searchPane.SuggestionsRequested += OnSearchPaneSuggestionsRequested;

            var sensor = SimpleOrientationSensor.GetDefault();
            if (sensor != null)
            {
                sensor.OrientationChanged += Sensor_OrientationChanged; ;
            }
        }

        private void Sensor_OrientationChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
        {
            string orientation = string.Empty;
            if (args.Orientation == SimpleOrientation.Rotated270DegreesCounterclockwise || args.Orientation == SimpleOrientation.Rotated90DegreesCounterclockwise)
            {
                orientation = ApplicationViewState.FullScreenPortrait.ToString();
            }
            else
            {
                orientation = ApplicationViewState.FullScreenLandscape.ToString();
            }

            Control control = this._rootFrame.Content as Control;
            if (control != null)
            {
                VisualStateManager.GoToState(control, orientation, false);
            }
        }

        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");


                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            _rootFrame = rootFrame;

            if (e.PrelaunchActivated == false)
            {
                await ShowSplash(e.SplashScreen);

                var bootstrapper = new Bootstrapper();
                bootstrapper.Finished += Bootstrapper_Finished;
                bootstrapper.Start();

                Window.Current.Activate();
            }
        }

        private async static Task ShowSplash(SplashScreen splashScreen)
        {
            Splash splash = new Splash(splashScreen);
            Window.Current.Content = splash;
            Window.Current.Activate();
            await Task.Delay(2000);
        }

        private void Bootstrapper_Finished(object sender, EventArgs e)
        {
            if (!_rootFrame.Navigate(typeof(TripListPage), false))
            {
                throw new Exception("Failed to create initial page");
            }

            Window.Current.Content = _rootFrame;

            // Ensure the current window is active
            Window.Current.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }
    }
}
