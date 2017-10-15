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
using Windows.UI.Xaml.Controls;

namespace BlueYonder.Companion.Client.ViewModels
{
    public class PurchaseViewModel : ViewModel
    {
        public DelegateCommand PurchaseCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        private TravelerInfoViewModel _travelerInfo;
        public TravelerInfoViewModel TravelerInfo
        {
            get { return this._travelerInfo; }
            set { this.SetProperty(ref this._travelerInfo, value); }
        }

        private Schedule _depart;
        public Schedule Depart
        {
            get { return this._depart; }
            set
            {
                this.SetProperty(ref this._depart, value);
                UpdateReturn();
            }
        }

        private Schedule _return;
        public Schedule Return
        {
            get { return this._return; }
            set { this.SetProperty(ref this._return, value); }
        }

        private int _flightId;
        public int FlightId
        {
            get { return this._flightId; }
            set { this.SetProperty(ref this._flightId, value); }
        }

        private Flight _departureFlight;
        public Flight DepartureFlight
        {
            get { return this._departureFlight; }
            set { this.SetProperty(ref this._departureFlight, value); }
        }

        private Flight _returnFlight;
        public Flight ReturnFlight
        {
            get { return this._returnFlight; }
            set { this.SetProperty(ref this._returnFlight, value); }
        }

        private IEnumerable<Schedule> _departureSchedules;
        public IEnumerable<Schedule> DepartureSchedules
        {
            get { return this._departureSchedules; }
            set { this.SetProperty(ref this._departureSchedules, value); }
        }

        private IEnumerable<Schedule> _returnSchedules;
        public IEnumerable<Schedule> ReturnSchedules
        {
            get { return this._returnSchedules; }
            set { this.SetProperty(ref this._returnSchedules, value); }
        }

        private bool _isRoundTrip;
        public bool IsRoundTrip
        {
            get { return this._isRoundTrip; }
            set
            {
                this.SetProperty(ref this._isRoundTrip, value);
                UpdateReturn();
            }
        }

        private readonly DataManager _data;
        private bool _isPurchaseCommandEnabled;

        public PurchaseViewModel()
        {
            this._data = new DataManager();

            this.TravelerInfo = new TravelerInfoViewModel();

            PurchaseCommand = new DelegateCommand(Purchase, CanExecutePurchase);
            CancelCommand = new DelegateCommand(Cancel);

            TogglePurchaseCommandEnabled(false);
        }

        public override async void Initialize(Frame frame)
        {
            base.Initialize(frame);

            this.TravelerInfo.Initialize(frame);

            var departureFlight = await _data.GetFlightByIdAsync(this.FlightId);
            UpdateDeparture(departureFlight);

            TogglePurchaseCommandEnabled(true);
        }

        private bool IsValid()
        {
            return this.TravelerInfo.IsValid();
        }

        private void Cancel(object parameter)
        {
            Frame.GoBack();
        }

        private async void Purchase(object parameter)
        {
            try
            {
                TogglePurchaseCommandEnabled(false);

                if (IsValid())
                {
                    this.TravelerInfo.Save(parameter);
                    var reservation = new Reservation()
                    {
                        TravelerId = UserAuth.Instance.Traveler.TravelerId,
                        ReservationDate = DateTime.Now,
                        DepartureFlight = new FlightSchedule()
                        {
                            FlightInfo = new FlightInfo()
                            {
                                Departure = this.Depart.Departure,
                                FlightScheduleId = this.Depart.FlightScheduleId
                            }
                        },
                    };

                    if (this.Return != null)
                    {
                        reservation.ReturnFlight = new FlightSchedule()
                        {
                            FlightInfo = new FlightInfo()
                            {
                                Departure = this.Return.Departure,
                                FlightScheduleId = this.Return.FlightScheduleId
                            }
                        };
                    }

                    var response = await _data.CreateNewReservationAsync(reservation);
                    if (response != null)
                    {
                        CacheManager.Invalidate(CacheType.Categories);
                        CacheManager.Invalidate(CacheType.Reservations);

                        var msg = new Windows.UI.Popups.MessageDialog(ResourceHelper.ResourceLoader.GetString("FlightPurchaseCompleted"));
                        await msg.ShowAsync();
                    }

                    this.Frame.Navigate(typeof (TripListPage), true);
                }
                else
                {
                    var msg = new Windows.UI.Popups.MessageDialog(ResourceHelper.ResourceLoader.GetString("FlightPurchaseMissingFields"));
                    await msg.ShowAsync();
                }
            }
            finally
            {
                TogglePurchaseCommandEnabled(true);
            }
        }

        private void TogglePurchaseCommandEnabled(bool enabled)
        {
            _isPurchaseCommandEnabled = enabled;
            PurchaseCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecutePurchase(object obj)
        {
            return _isPurchaseCommandEnabled;
        }

        private void UpdateDeparture(Flight flight)
        {
            var schedules = GetPotentialSchedules(flight, DateTime.Now);
            this.DepartureFlight = flight;
            this.DepartureSchedules = schedules;
            this.Depart = schedules.FirstOrDefault();
        }

        private async void UpdateReturn()
        {
            if (this.IsRoundTrip)
            {
                var departureFlight = this.DepartureFlight;
                var startDate = this.Depart.Departure.Value.AddHours(24);
                var returnFlights = await _data.GetFlightsAsync(departureFlight.Destination.LocationId, departureFlight.Source.LocationId, startDate);
                var returnFlight = returnFlights.First();

                var schedules = GetPotentialSchedules(returnFlight, startDate);
                this.ReturnFlight = returnFlight;
                this.ReturnSchedules = schedules;
                this.Return = schedules.FirstOrDefault();
            }
            else
            {
                this.ReturnFlight = null;
                this.ReturnSchedules = null;
                this.Return = null;
            }

        }

        private static IEnumerable<Schedule> GetPotentialSchedules(Flight flight, DateTime after)
        {
            return
                flight
                    .Schedules
                    .OrderBy(f => f.Departure.Value)
                    .Where(f => f.Departure.Value > after)
                    .Take(3)
                    .ToArray();
        }
    }
}
