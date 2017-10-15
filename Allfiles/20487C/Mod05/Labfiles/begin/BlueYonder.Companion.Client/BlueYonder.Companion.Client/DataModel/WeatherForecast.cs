using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueYonder.Companion.Client.DataModel
{
    public class WeatherForecast
    {
        public double TemperatureCelsius { get; set; }
        public double TemperatureFahrenheit { get; set; }
        public WeatherCondition Condition { get; set; }
    }
}
