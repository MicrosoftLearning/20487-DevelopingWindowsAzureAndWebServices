using System;
using Windows.ApplicationModel.Background;

namespace BlueYonder.Companion.Client.Helpers
{
    public class Bootstrapper
    {
        public event EventHandler Finished;

        public async void Start()
        {
            var loginResult = await UserAuth.Instance.Login();
            if (loginResult.IsLoggedIn)
            {
                await ReservationDataFetcher.Instance.GetCategoriesAsync(false);
            }

            try
            {
                var wnsManager = new WnsManager();
                await wnsManager.Register();
            }
            catch (Exception e)
            {
                
            }

            var transferManager = new TransferManager();
            await transferManager.ResumeDownloadsAsync();

            await BackgroundExecutionManager.RequestAccessAsync();
            BackgroundTaskHelper.RegisterBackgroundTaskForWeather();

           // TODO: Module 12: Exercise 1: Task 2.3: Load the license data
            await LicenseManager.Instance.LoadLicenseData();

            FireFinishedEvent();
        }

        private void FireFinishedEvent()
        {
            var handler = Finished;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}