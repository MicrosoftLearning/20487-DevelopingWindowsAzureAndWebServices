using System.Collections.Generic;
using BlueYonder.Companion.Client.DataModel;

namespace BlueYonder.Companion.Client.Helpers
{
    public class LocationsFetchedEventArgs
    {
        public string QueryText { get; private set; }
        public Location[] Locations { get; private set; }

        public LocationsFetchedEventArgs(string queryText, Location[] locations)
        {
            QueryText = queryText;
            Locations = locations;
        }
    }
}