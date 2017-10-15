using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueYonder.Companion.Client.Helpers;

namespace BlueYonder.Companion.Client.DataModel
{
    public class Schedule
    {
        public DateTime? Departure  { get; set; }
        public DateTime? Arrival { get; set; }
        public TimeSpan? Duration { get; set; }
        public int FlightScheduleId { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1} {2}: {3} {4}: {5}", 
                ResourceHelper.ResourceLoader.GetString("Departure"), this.Departure,
                ResourceHelper.ResourceLoader.GetString("Arrival"), this.Arrival,
                ResourceHelper.ResourceLoader.GetString("Duration"), this.Duration);
        }
    }
}
