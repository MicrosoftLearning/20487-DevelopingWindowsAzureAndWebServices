using BlueYonder.Companion.Client.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueYonder.Companion.Client.Helpers;
using BlueYonder.Companion.Client.Common;
using System.Collections.ObjectModel;

namespace BlueYonder.Companion.Client.Helpers
{
    public class ReservationDataFetcher : DataFetcher
    {
        private readonly DataManager _data;
        private IEnumerable<Reservation> _reservations;
        private readonly ObservableCollection<TripCategory> _trips;

        private readonly TripCategory _currentTrips;
        private readonly TripCategory _upcomingTrips;
        private readonly TripCategory _historicTrips;

        private ReservationDataFetcher()
        {
            _data = new DataManager();

            _reservations = new ObservableCollection<Reservation>();
            _trips = new ObservableCollection<TripCategory>();

            _currentTrips = new TripCategory(ResourceHelper.ResourceLoader.GetString("CurrentTrip"), CategoryType.CurrentTrip);
            _upcomingTrips = new TripCategory(ResourceHelper.ResourceLoader.GetString("UpcomingTrips"), CategoryType.UpcomingTrips);
            _historicTrips = new TripCategory(ResourceHelper.ResourceLoader.GetString("HistoricTrips"), CategoryType.HistoricTrips);

            this._trips.Add(_currentTrips);
            this._trips.Add(_upcomingTrips);
            this._trips.Add(_historicTrips);
        }

        public async Task<ObservableCollection<TripCategory>> GetCategoriesAsync(bool forceRefresh = false)
        {
            var reservations = await GetReservationsAsync(forceRefresh);
            return _trips;
        }

        public async Task<ObservableCollection<Reservation>> GetReservationByCategoryAsyc(CategoryType type, bool forceRefresh = false)
        {
            var reservations = await GetReservationsAsync(forceRefresh);
            switch (type)
            {
                case CategoryType.CurrentTrip:
                    return _currentTrips.Items;
                case CategoryType.UpcomingTrips:
                    return _upcomingTrips.Items;
                default:
                    return _historicTrips.Items;
            }
        }

        private async Task BuildReservationCategories()
        {
            _currentTrips.Items.Clear();
            _upcomingTrips.Items.Clear();
            _historicTrips.Items.Clear();

            var trips = _reservations.ToList();

            Reservation currentTrip = null;
            foreach (var trip in trips)
            {
                if (IsHistoricTrip(trip))
                {
                    trip.Type = CategoryType.HistoricTrips;
                    _historicTrips.Items.Add(trip);
                }
                else if (currentTrip == null && IsCurrentTrip(trip))
                {
                    currentTrip = trip;
                    trip.Type = CategoryType.CurrentTrip;
                    _currentTrips.Items.Add(trip);
                }
                else
                {
                    trip.Type = CategoryType.UpcomingTrips;
                    _upcomingTrips.Items.Add(trip);
                }
            }

            await CacheManager.Save(CacheType.Categories, _trips);
        }

        private async Task<IEnumerable<Reservation>> GetReservationsAsync(bool forceRefresh)
        {
            if (_reservations == null || _reservations.Count() == 0 || forceRefresh)
            {
                _reservations = await _data.GetReservationsAsync(UserAuth.Instance.Traveler.TravelerId);

                await CacheManager.Save(CacheType.Reservations, _reservations);

                await BuildReservationCategories();

                LastRefreshDateTime = DateTime.Now;
            }

            return _reservations;
        }

        private static ReservationDataFetcher _instance;
        public static ReservationDataFetcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ReservationDataFetcher();
                }
                return _instance;
            }
        }

        private static bool IsHistoricTrip(Reservation trip)
        {
            return IsInCategory(trip, (now, start, end) => start < now && end < now);
        }

        private static bool IsCurrentTrip(Reservation trip)
        {
            return IsInCategory(trip, (now, start, end) => start <= now && now < end);
        }

        private static bool IsInCategory(Reservation trip, Func<DateTime, DateTime, DateTime, bool> tripPredicate)
        {
            var start = trip.DepartureFlight.FlightInfo.Departure.GetValueOrDefault().AddDays(-1);
            var endFlight = trip.ReturnFlight ?? trip.DepartureFlight;
            var end = endFlight.FlightInfo.Departure.GetValueOrDefault().AddDays(1);
            var isInCategory = tripPredicate(DateTime.Now, start, end);
            return isInCategory;
        }
    }
}
