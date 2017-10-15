using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueYonder.Companion.Client.DataModel
{
    public class Flight
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; }
        public int FrequentFlyerMiles { get; set; }

        public Location Source { get; set; }
        public Location Destination { get; set; }

        public List<Schedule> Schedules { get; set; }
    }
}
