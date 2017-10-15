using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using BlueYonder.Companion.Client.Common;

namespace BlueYonder.Companion.Client.Helpers
{
    public class GeopositionDataFetcher : DataFetcher
    {
        private static GeopositionDataFetcher _instance;
        public static GeopositionDataFetcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GeopositionDataFetcher();
                }
                return _instance;
            }
        }

        private readonly Geolocator _geolocator;
        private Geoposition _geoposition;

        private GeopositionDataFetcher()
        {
            _geolocator = new Geolocator();
        }

        public async Task<Geoposition> GetLocationAsync()
        {
            if (RequireRefresh)
            {
                try
                {
                    _geoposition = await _geolocator.GetGeopositionAsync();
                }
                catch
                {
                    // Probably an HRESULT E_FAIL exception of unclear origin
                }
                LastRefreshDateTime = DateTime.Now;
            }
            return _geoposition;
        } 
    }
}
