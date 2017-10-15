using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueYonder.Companion.Client.DataTransferObjects
{
    public class RegisterNotificationsRequestDTO
    {
        public string DeviceID { get; set; }
        public string DeviceURI { get; set; }
    }
}