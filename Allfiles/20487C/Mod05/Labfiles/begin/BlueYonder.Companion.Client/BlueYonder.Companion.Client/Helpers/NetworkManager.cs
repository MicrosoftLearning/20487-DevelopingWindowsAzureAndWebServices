using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;

namespace BlueYonder.Companion.Client.Helpers
{
    class NetworkManager
    {
        public static bool IsNetworkAvailable
        {
            get
            {
                ConnectionProfile connectionProfile = NetworkInformation.GetInternetConnectionProfile();
                return connectionProfile != null;
            }
        }

        public static async Task<bool> CheckInternetConnection(bool displayMessage, string message)
        {
            var isConnected = IsNetworkAvailable;
            if (!isConnected && displayMessage)
            {
                var dialog = new MessageDialog(message, ResourceHelper.ResourceLoader.GetString("NoInternetConnection"));
                await dialog.ShowAsync();
            }
            return isConnected;
        }
    }
}
