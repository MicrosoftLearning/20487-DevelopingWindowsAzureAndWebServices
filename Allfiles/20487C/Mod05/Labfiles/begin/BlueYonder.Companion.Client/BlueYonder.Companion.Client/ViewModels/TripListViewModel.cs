using BlueYonder.Companion.Client.Common;
using BlueYonder.Companion.Client.DataModel;
using BlueYonder.Companion.Client.Helpers;
using BlueYonder.Companion.Client.Views.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Search;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BlueYonder.Companion.Client.ViewModels
{
    public class TripListViewModel : ViewModel
    {
        public DelegateCommand LoginCommand { get; set; }
        public DelegateCommand LogoutCommand { get; set; }

        private const string CurrentTripMediaFolder = "ms-appdata:///temp/CurrentMedia";

        private readonly DataManager _data;

        private ObservableCollection<TripCategory> _tripCategories;
        public ObservableCollection<TripCategory> TripCategories
        {
            get { return this._tripCategories; }
            private set { this.SetProperty(ref this._tripCategories, value); }
        }

        private Reservation _currentTrip;
        public Reservation CurrentTrip
        {
            get { return this._currentTrip; }
            set
            {
                if (this.SetProperty(ref this._currentTrip, value))
                {
                    FillCurrentTripMediaItems();
                }
            }
        }

        private bool _working;
        public bool Working
        {
            get { return this._working; }
            set { this.SetProperty(ref this._working, value); }
        }

        private WeatherForecast _weather;
        public WeatherForecast Weather
        {
            get { return this._weather; }
            set { this.SetProperty(ref this._weather, value); }
        }

        private bool _isLoginCommandVisible;
        public bool IsLoginCommandVisible
        {
            get { return this._isLoginCommandVisible; }
            set { this.SetProperty(ref this._isLoginCommandVisible, value); }
        }

        private bool _isLogoutCommandVisible;
        public bool IsLogoutCommandVisible
        {
            get { return this._isLogoutCommandVisible; }
            set { this.SetProperty(ref this._isLogoutCommandVisible, value); }
        }

        private bool _isTripDataVisible;
        public bool IsTripDataVisible
        {
            get { return this._isTripDataVisible; }
            set { this.SetProperty(ref this._isTripDataVisible, value); }
        }

        private bool _isGlanceVisible;
        public bool IsGlanceVisible
        {
            get { return this._isGlanceVisible; }
            set { this.SetProperty(ref this._isGlanceVisible, value); }
        }

        private TravelerInfoViewModel _travelerInfo;
        public TravelerInfoViewModel TravelerInfo
        {
            get { return this._travelerInfo; }
            set { this.SetProperty(ref this._travelerInfo, value); }
        }

        private bool _forceRefresh;
        public bool ForceRefresh
        {
            get { return this._forceRefresh; }
            set { this.SetProperty(ref this._forceRefresh, value); }
        }

        private ICollection<MediaItem> _currentDestinationImages;
        public ICollection<MediaItem> CurrentDestinationImages
        {
            get { return this._currentDestinationImages; }
            private set { this.SetProperty(ref this._currentDestinationImages, value); }
        }

       
        private bool _isTrialLicense;
        public bool IsTrialLicense
        {
            get { return this._isTrialLicense; }
            set { this.SetProperty(ref this._isTrialLicense, value); }
        }

        public TripListViewModel()
        {
            this._data = new DataManager();

            this.TripCategories = new ObservableCollection<TripCategory>();
            this.CurrentDestinationImages = new ObservableCollection<MediaItem>();

            this.TravelerInfo = new TravelerInfoViewModel();

            IsLoginCommandVisible = false;
            IsLogoutCommandVisible = false;
            IsTripDataVisible = false;
            IsGlanceVisible = false;
            LoginCommand = new DelegateCommand(Login);
            LogoutCommand = new DelegateCommand(Logout);
            // TODO: Module 12: Exercise 1: Task 2.5: Initialize the IsTrialLicense property
            IsTrialLicense = LicenseManager.Instance.IsTrialLicense;
        }

        public override async void Initialize(Frame frame)
        {
            base.Initialize(frame);

            TravelerInfo.Initialize(frame);

            //Initialize the storage folder and TransferManager for the public images of the current trip
            StorageFolder tempRoot = ApplicationData.Current.TemporaryFolder;
            _tempMediaFolder = await tempRoot.CreateFolderAsync("CurrentMedia", CreationCollisionOption.OpenIfExists);
            _transferManager = new TransferManager();
            EmptyTempFolder();

            var isLoggedIn = UserAuth.Instance.IsLoggedIn;

            IsTripDataVisible = true;
            IsLoginCommandVisible = false;
            IsLogoutCommandVisible = false;

            await LoadTrips(isLoggedIn);
            await InitializeWeather(CurrentTrip);

            // TODO: Module 12: Exercise 1: Task 2.5: Subscribe to the LicenseManager.LicenseDataUpdated event
            LicenseManager.Instance.LicenseDataUpdated += LicenseManager_LicenseDataUpdated;
        }

        private void InitializeCurrentTrip(bool isLoggedIn)
        {
            var currentTripCategory = TripCategories.FirstOrDefault(t => t.Type == CategoryType.CurrentTrip);
            var currentTrip =
                !isLoggedIn || currentTripCategory == null || currentTripCategory.Items.Count == 0
                    ? null
                    : currentTripCategory.Items[0];

            CurrentTrip = currentTrip;
            IsGlanceVisible = currentTrip != null;
        }

        public override void Uninitialize()
        {
            base.Uninitialize();

            UninitializeWeather();

            // TODO: Module 12: Exercise 1: Task 2.5: Unsubscribe from the LicenseManager.LicenseDataUpdated event
            LicenseManager.Instance.LicenseDataUpdated -= LicenseManager_LicenseDataUpdated;
        }

        private async Task LoadTrips(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                var cacheItem = await CacheManager.Load<ObservableCollection<TripCategory>>(CacheType.Categories);
                if (cacheItem != null && cacheItem.Data != null)
                {
                    TripCategories = (ObservableCollection<TripCategory>) cacheItem.Data;
                }

                if (cacheItem == null || cacheItem.Data == null || cacheItem.RequireRefresh)
                {
                    await GetTravelerFlightsFromServer(ForceRefresh);
                }
            }
            InitializeCurrentTrip(isLoggedIn);
        }

        private async Task GetTravelerFlightsFromServer(bool forceRefresh)
        {
            Working = true;
            TripCategories = await ReservationDataFetcher.Instance.GetCategoriesAsync(forceRefresh);
            Working = false;
        }

        private async void Login(object parameter)
        {
        }

        private void Logout(object parameter)
        {
        }

        private async Task InitializeWeather(Reservation currentTrip)
        {
            WeatherForecast weather = null;
            if (currentTrip != null)
            {
                var flightInfo = currentTrip.DepartureFlight.FlightInfo;
                var locationId = flightInfo.Flight.Destination.LocationId;
                var departure = flightInfo.Departure ?? DateTime.Now;

                weather = await _data.GetWeatherForecastByIdAsync(locationId, departure);

                ApplicationData.Current.LocalSettings.Values["locationId"] = locationId;
                ApplicationData.Current.LocalSettings.Values["locationName"] = flightInfo.Flight.Source.City;
                ApplicationData.Current.LocalSettings.Values["departure"] = string.Format("{0:yyyy-MM-dd}", departure);
            }

            Weather = weather;

            var weatherUpdateTask = BackgroundTaskHelper.FindRegisteredTask(Constants.WeatherTaskName);
            if (weatherUpdateTask != null)
            {
                weatherUpdateTask.Completed += WeatherUpdateTaskOnCompleted;
            }
        }

        private void UninitializeWeather()
        {
            var weatherUpdateTask = BackgroundTaskHelper.FindRegisteredTask(Constants.WeatherTaskName);
            if (weatherUpdateTask != null)
            {
                weatherUpdateTask.Completed -= WeatherUpdateTaskOnCompleted;
            }
        }

        private void WeatherUpdateTaskOnCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            try
            {
                args.CheckResult();

                WeatherCondition condition;
                Enum.TryParse(GetSetting("weather.condition"), out condition);

                double celsius;
                double.TryParse(GetSetting("weather.celsius"), out celsius);

                double fahrenheit;
                double.TryParse(GetSetting("weather.fahrenheit"), out fahrenheit);

                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Weather = new WeatherForecast()
                    {
                        Condition = condition,
                        TemperatureCelsius = celsius,
                        TemperatureFahrenheit = fahrenheit
                    };
                });
            }
            catch (Exception exc)
            {
                new MessageDialog(exc.Message).ShowAsync();
            }
        }

        private static string GetSetting(string key)
        {
            var settings = ApplicationData.Current.LocalSettings;
            var value = settings.Values[key];
            return value == null ? null : value.ToString();
        }

        private StorageFolder _tempMediaFolder;
        private TransferManager _transferManager;

        private async void FillCurrentTripMediaItems()
        {
            if (CurrentTrip == null)
            {
                CurrentDestinationImages.Clear();
                return;
            }

            var locationId = CurrentTrip.DepartureFlight.FlightInfo.Flight.Destination.LocationId;
            var files = await _data.GetAzureStorageFilesByLocation(locationId, 5);
            foreach (var fileMetadata in files)
            {
                StorageFile destinationFile = await _tempMediaFolder.CreateFileAsync(fileMetadata.FileName, CreationCollisionOption.ReplaceExisting);

                await _transferManager.DownloadAsync(fileMetadata.Uri, destinationFile);

                var mediaItem = new MediaItem(FolderType.Temp, CurrentTripMediaFolder, fileMetadata.FileName);
                CurrentDestinationImages.Add(mediaItem);
            }
        }

        private async void EmptyTempFolder()
        {
            var files = await _tempMediaFolder.GetFilesAsync();
            foreach (var file in files)
            {
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        // TODO: Module 12: Exercise 1: Task 2.5: Handle the LicenseDataUpdated event
        private void LicenseManager_LicenseDataUpdated(object sender, EventArgs e)
        {
            IsTrialLicense = LicenseManager.Instance.IsTrialLicense;
        }
    }
}
