using BlueYonder.Companion.Client.Common;
using BlueYonder.Companion.Client.DataModel;
using BlueYonder.Companion.Client.Helpers;
using BlueYonder.Companion.Client.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Geolocation;

namespace BlueYonder.Companion.Client.ViewModels
{
    public class TripDetailViewModel : ViewModel
    {
        private readonly DataManager _data;

        public DelegateCommand PurchaseCommand { get; set; }
        public DelegateCommand PinCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand CheckInCommand { get; set; }
        public DelegateCommand MediaCommand { get; set; }

        public DelegateCommand PrintBoardingPassCommand { get; set; }
        public DelegateCommand PrintReceiptCommand { get; set; }

        public LayoutAwarePage Page { get; set; }

        private CategoryType _categoryType;
        public CategoryType CategoryType
        {
            get { return this._categoryType; }
            set { this.SetProperty(ref this._categoryType, value); }
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get { return this._searchQuery; }
            set
            {
                this.SetProperty(ref this._searchQuery, value);
                UpdateVisibility();
            }
        }

        private Reservation _selectedReservation;
        public Reservation SelectedReservation
        {
            get { return this._selectedReservation; }
            set
            {
                this.SetProperty(ref this._selectedReservation, value);
                UpdateSelectedReservationDetails(value);
                UpdateVisibility();
            }
        }

        private string _selectedReservationCaption;
        public string SelectedReservationCaption
        {
            get { return this._selectedReservationCaption; }
            set { this.SetProperty(ref this._selectedReservationCaption, value); }
        }

        private string _selectedReservationFlightNumber;
        public string SelectedReservationFlightNumber
        {
            get { return this._selectedReservationFlightNumber; }
            set { this.SetProperty(ref this._selectedReservationFlightNumber, value); }
        }

        private bool _hasDepartingDate;
        public bool HasDepartingDate
        {
            get { return this._hasDepartingDate; }
            set { this.SetProperty(ref this._hasDepartingDate, value); }
        }

        private bool _hasReturningDate;
        public bool HasReturningDate
        {
            get { return this._hasReturningDate; }
            set { this.SetProperty(ref this._hasReturningDate, value); }
        }

        private bool _isPurchaseCommandVisible;
        public bool IsPurchaseCommandVisible
        {
            get { return this._isPurchaseCommandVisible; }
            set { this.SetProperty(ref this._isPurchaseCommandVisible, value); }
        }

        private bool _isPinCommandVisible;
        public bool IsPinCommandVisible
        {
            get { return this._isPinCommandVisible; }
            set { this.SetProperty(ref this._isPinCommandVisible, value); }
        }

        private bool _canPurchaseTripAddOn;
        public bool CanPurchaseTripAddOn
        {
            get { return this._canPurchaseTripAddOn; }
            set { this.SetProperty(ref this._canPurchaseTripAddOn, value); }
        }

        private string _fastSecurityScreeningPrice;
        public string FastSecurityScreeningPrice
        {
            get { return this._fastSecurityScreeningPrice; }
            set { this.SetProperty(ref this._fastSecurityScreeningPrice, value); }
        }

        private string _priorityBaggageHandlingPrice;
        public string PriorityBaggageHandlingPrice
        {
            get { return this._priorityBaggageHandlingPrice; }
            set { this.SetProperty(ref this._priorityBaggageHandlingPrice, value); }
        }

        private string _fareClassUpgradePrice;
        public string FareClassUpgradePrice
        {
            get { return this._fareClassUpgradePrice; }
            set { this.SetProperty(ref this._fareClassUpgradePrice, value); }
        }

        private bool _isMediaCommandVisible;
        public bool IsMediaCommandVisible
        {
            get { return this._isMediaCommandVisible; }
            set { this.SetProperty(ref this._isMediaCommandVisible, value); }
        }

        private bool _isLoginCommandVisible;
        public bool IsLoginCommandVisible
        {
            get { return this._isLoginCommandVisible; }
            set { this.SetProperty(ref this._isLoginCommandVisible, value); }
        }

        public int? ReservationId { get; set; }

        private ObservableCollection<Reservation> _reservations = new ObservableCollection<Reservation>();
        public ObservableCollection<Reservation> Reservations
        {
            get { return this._reservations; }
            set { this.SetProperty(ref this._reservations, value); }
        }

        private TravelerInfoViewModel _travelerInfo;
        public TravelerInfoViewModel TravelerInfo
        {
            get { return this._travelerInfo; }
            set { this.SetProperty(ref this._travelerInfo, value); }
        }


        private FlightSchedule _departureFlight;
        public FlightSchedule DepartureFlight {
            get => _departureFlight;
            set => SetProperty(ref _departureFlight, value);
        }

        private FlightSchedule _returnFlight;

        public FlightSchedule ReturnFlight
        {
            get => _returnFlight;
            set => SetProperty(ref _returnFlight, value);
        }

        private string _confirmationCode;
        public string ConfirmationCode
        {
            get => _confirmationCode;
            set => SetProperty(ref _confirmationCode, value);
        }

        private void UpdateSelectedReservationDetails(Reservation reservation)
        {
            if (reservation != null)
            {
                var flight = reservation.DepartureFlight.FlightInfo.Flight;
                var localizedSource = ResourceHelper.ResourceLoader.GetString("City_" + flight.Source.LocationId);
                var localizedDestination = ResourceHelper.ResourceLoader.GetString("City_" + flight.Destination.LocationId);
                var formatString = ResourceHelper.ResourceLoader.GetString("SourceToDestination");
                SelectedReservationCaption = string.Format(formatString, localizedSource, localizedDestination);

                SelectedReservationFlightNumber = flight.FlightNumber;
            }
            else
            {
                SelectedReservationCaption = string.Empty;

                SelectedReservationFlightNumber = string.Empty;
            }
        }

        private void UpdateVisibility()
        {
            HasDepartingDate = this.SelectedReservation != null && this.SelectedReservation.DepartureFlight != null;
            HasReturningDate = this.SelectedReservation != null && this.SelectedReservation.ReturnFlight != null;
            IsPurchaseCommandVisible = this.CategoryType == CategoryType.SearchResult && this.SelectedReservation != null;
            IsPinCommandVisible = this.CategoryType != CategoryType.SearchResult && this.SelectedReservation != null;
            CanPurchaseTripAddOn = this.SelectedReservation != null && this.SelectedReservation.Type == CategoryType.CurrentTrip;
            IsMediaCommandVisible = this.CategoryType != CategoryType.SearchResult && this.SelectedReservation != null;
        }

        public TripDetailViewModel()
        {
            this._data = new DataManager();

            this.TravelerInfo = new TravelerInfoViewModel();

            FastSecurityScreeningPrice = FormatCurrency(100);
            PriorityBaggageHandlingPrice = FormatCurrency(50);
            FareClassUpgradePrice = FormatCurrency(300);

            PurchaseCommand = new DelegateCommand(Purchase);
            PinCommand = new DelegateCommand(Pin);
            CancelCommand = new DelegateCommand(CancelTrip);
            CheckInCommand = new DelegateCommand(CheckIn);

            MediaCommand = new DelegateCommand(Media, CanExecuteMedia);

            PrintBoardingPassCommand = new DelegateCommand(PrintBoardingPass);
            PrintReceiptCommand = new DelegateCommand(PrintReceipt);

            IsLoginCommandVisible = false;
        }

        private static string FormatCurrency(double price)
        {
            var currency = ResourceHelper.ResourceLoader.GetString("Currency");
            var currencyFormatter = new Windows.Globalization.NumberFormatting.CurrencyFormatter(currency);
            var formattedCurrency = currencyFormatter.Format(price);
            return formattedCurrency;
        }

        public override void Initialize(Frame frame)
        {
            base.Initialize(frame);

            if (this.CategoryType == CategoryType.SearchResult)
            {
                LocationsDataFetcher.Instance.LocationsFetched += LocationsFetched;
                LoadFlightsByQuery(this.SearchQuery, true);
            }
            else
            {
                LoadFlightsAsync(false);
            }

            // TODO: Module 12: Exercise 2: Task 2.2 Subscribe to the LicenseDataUpdated event
            LicenseManager.Instance.LicenseDataUpdated += LicenseManager_LicenseDataUpdated;
            IsLoginCommandVisible = false;
        }

        public override void Uninitialize()
        {
            base.Uninitialize();

            if (CategoryType == CategoryType.SearchResult)
            {
                LocationsDataFetcher.Instance.LocationsFetched -= LocationsFetched;
            }

            // TODO: Module 12: Exercise 2: Task 2.2 Unsubscribe from the LicenseDataUpdated event
            LicenseManager.Instance.LicenseDataUpdated -= LicenseManager_LicenseDataUpdated;
        }

        private async void LoadFlightsAsync(bool forceRefresh)
        {
            this.SearchQuery = string.Empty;
            this.Reservations = await ReservationDataFetcher.Instance.GetReservationByCategoryAsyc(this.CategoryType, forceRefresh);
            if (this.ReservationId != null)
            {
                this.SelectedReservation = this.Reservations.FirstOrDefault(res => res.ReservationId.Equals(this.ReservationId));
            }
            else
            {
                this.SelectedReservation = this.Reservations.FirstOrDefault();
            }
        }

        private async void LoadFlightsByQuery(string query, bool force)
        {
            await LocationsDataFetcher.Instance.FetchLocationsAsync(query, force);
        }

        private async void LocationsFetched(object sender, LocationsFetchedEventArgs e)
        {
            this.Reservations.Clear();
            this.SearchQuery = e.QueryText;
            this.SelectedReservation = null;

            var geoposition = await GeopositionDataFetcher.Instance.GetLocationAsync();
            var geocoordinate = geoposition == null ? null : geoposition.Coordinate;
            var sourceLocation = LocationsDataFetcher.Instance.GetLocationByCoordinate(geocoordinate);

            if (sourceLocation != null)
            {
                var startDate = DateTime.Now.AddHours(12);

                foreach (var destination in e.Locations)
                {
                    var flights = await this._data.GetFlightsAsync(sourceLocation.LocationId, destination.LocationId, startDate);
                    if (flights == null)
                    {
                        flights = new List<Flight>();
                    }

                    foreach (var flight in flights)
                    {
                        Reservation reservation = new Reservation
                        {
                            DepartureFlight = new FlightSchedule
                            {
                                FlightInfo = new FlightInfo
                                {
                                    Flight = flight,
                                    Departure = flight.Schedules.OrderBy(f => f.Departure).First().Departure
                                }
                            }
                        };
                        this.Reservations.Add(reservation);
                    }
                }
            }
            this.SelectedReservation = this.Reservations.FirstOrDefault();
        }

        private void Purchase(object parameter)
        {
            this.Frame.Navigate(typeof(PurchasePage), this.SelectedReservation.DepartureFlight.FlightInfo.Flight.FlightId);
        }

        private async void Pin(object parameter)
        {
            await TileManager.Pin(this, this.SelectedReservation);
        }

        private async void CancelTrip(object parameter)
        {
            if (this.SelectedReservation != null)
            {
                var msg = new Windows.UI.Popups.MessageDialog(ResourceHelper.ResourceLoader.GetString("CancelTripDialog"));
                msg.Commands.Add(new Windows.UI.Popups.UICommand(ResourceHelper.ResourceLoader.GetString("Yes"), new Windows.UI.Popups.UICommandInvokedHandler(this.DeleteReservation)));
                msg.Commands.Add(new Windows.UI.Popups.UICommand(ResourceHelper.ResourceLoader.GetString("No")));
                await msg.ShowAsync();
            }
            else
            {
                var msg = new Windows.UI.Popups.MessageDialog(ResourceHelper.ResourceLoader.GetString("SelectFlightToDelete"));
                await msg.ShowAsync();
            }
        }

        private async void DeleteReservation(IUICommand command)
        {
            await _data.DeleteReservationAsync(this.SelectedReservation.ReservationId);
            LoadFlightsAsync(true);
        }

        private void CheckIn(object parameter)
        {
        }

        private void Media(object parameter)
        {
            Reservation reservation = this.SelectedReservation;
            int? reservationId = reservation == null ? (int?)null : reservation.ReservationId;
            this.Frame.Navigate(typeof(MediaPage), reservationId);
        }

        // TODO: Module 12: Exercise 2: Task 2.1: Implement the CanExecuteMedia method
        private bool CanExecuteMedia(object parameter)
        {
            return LicenseManager.Instance.IsMediaFeatureEnabled;
        }

        private async void PrintBoardingPass(object parameter)
        {
            if (this.SelectedReservation != null)
            {
                using (new PrinterJob(Page, PrintJobType.BoardingPass, this.SelectedReservation))
                {
                    await PrintManager.ShowPrintUIAsync();
                }
            }
            else
            {
                var msg = new MessageDialog(ResourceHelper.ResourceLoader.GetString("PrintMissingReservation"));
                await msg.ShowAsync();
            }
        }

        private async void PrintReceipt(object parameter)
        {
            if (this.SelectedReservation != null)
            {
                using (new PrinterJob(Page, PrintJobType.Receipt, this.SelectedReservation))
                {
                    await PrintManager.ShowPrintUIAsync();
                }
            }
            else
            {
                var msg = new MessageDialog(ResourceHelper.ResourceLoader.GetString("PrintMissingReservation"));
                await msg.ShowAsync();
            }
        }

        // TODO: Module 12: Exercise 2: Task 2.2: Handle the LicenseDataUpdated event and update the state of the MediaCommand
        private void LicenseManager_LicenseDataUpdated(object sender, EventArgs e)
        {
            MediaCommand.RaiseCanExecuteChanged();
        }
    }
}
