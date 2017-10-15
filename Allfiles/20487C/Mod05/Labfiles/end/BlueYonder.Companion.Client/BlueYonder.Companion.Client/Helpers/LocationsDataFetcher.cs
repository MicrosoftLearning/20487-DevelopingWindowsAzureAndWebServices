using BlueYonder.Companion.Client.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueYonder.Companion.Client.Helpers;
using BlueYonder.Companion.Client.Common;
using Windows.Devices.Geolocation;

namespace BlueYonder.Companion.Client.Helpers
{
    public class LocationsDataFetcher : DataFetcher
    {
        private readonly DataManager _data;
        public EventHandler<LocationsFetchedEventArgs> LocationsFetched;
        private string _lastQuery;
        private IEnumerable<Location> _lastQueryResults;
        private bool _isFetching;

        private LocationsDataFetcher()
        {
            _data = new DataManager();
        }

        public async Task<IEnumerable<Location>> FetchLocationsAsync(string query, bool force)
        {
            while (_isFetching)
            {
                await Task.Delay(100);
            }

            Location[] locations;
            try
            {
                _isFetching = true;

                if (!force && !IsNewSearch(query))
                {
                    return this._lastQueryResults;
                }

                locations = (await _data.GetLocationsAsync(query)).ToArray();

                this._lastQuery = query;
                this._lastQueryResults = locations;
                this.LastRefreshDateTime = DateTime.Now;

                var handler = LocationsFetched;
                if (handler != null)
                {
                    handler(this, new LocationsFetchedEventArgs(query, locations));
                }
            }
            finally
            {
                _isFetching = false;
            }

            return locations;
        }

        private bool IsNewSearch(string queryText)
        {
            return RequireRefresh || queryText != this._lastQuery;
        }

        private static LocationsDataFetcher _instance;
        public static LocationsDataFetcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LocationsDataFetcher();
                }
                return _instance;
            }
        }

        public Location GetLocationByCoordinate(Geocoordinate geocoordinate)
        {
            // seattle - 47° 36′ 35″ N, 122° 19′ 59″ W - 47.609722, -122.333056
            // paris - 48° 51′ 24.12″ N, 2° 21′ 2.88″ E - 48.8567, 2.3508
            // rome - 41° 54′ 0″ N, 12° 30′ 0″ E - 41.9, 12.5
            // new york city - 40° 39′ 51″ N, 73° 56′ 19″ W - 40.664167, -73.938611

            if (geocoordinate != null)
            {
                if (geocoordinate.Latitude == 47.609722 && geocoordinate.Longitude == -122.333056)
                {
                    return CreateLocation(1, "Seattle", "United States", "Washington");
                }
                else if (geocoordinate.Latitude == 48.8567 && geocoordinate.Longitude == 2.3508)
                {
                    return CreateLocation(2, "Paris", "France");
                }
                else if (geocoordinate.Latitude == 41.9 && geocoordinate.Longitude == 12.5)
                {
                    return CreateLocation(3, "Rome", "Italy");
                }
                else if (geocoordinate.Latitude == 40.664167 && geocoordinate.Longitude == -73.938611)
                {
                    return CreateLocation(4, "New York", "United States", "New York");
                }
            }

            // For the purposes of these labs, always default to Seattle
            return CreateLocation(1, "Seattle", "United States", "Washington");
        }

        private static Location CreateLocation(int locationId, string city, string country, string state = null)
        {
            return new Location()
            {
                LocationId = locationId,
                City = city,
                Country = country,
                State = state
            };
        }
    }
}
