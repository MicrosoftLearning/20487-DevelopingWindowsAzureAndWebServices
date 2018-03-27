using Microsoft.WindowsAzure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using Windows.Security.Cryptography;
using Windows.UI.Notifications;

namespace BlueYonder.Companion.Client.Helpers
{
    class WnsManager
    {
        private async Task<bool> SendChannelToServer(string channelUri)
        {
            var dataManager = new DataManager();
            return await dataManager.RegisterNotificationsChannelAsync(channelUri);
        }

        private string EncodeChannelUri(string uri)
        {
            var channelBuffer = CryptographicBuffer.ConvertStringToBinary(uri, BinaryStringEncoding.Utf8);
            return CryptographicBuffer.EncodeToBase64String(channelBuffer);
        }

        public async Task<bool> Register(UserAuth userAuth)
        {
            if (!NetworkManager.IsNetworkAvailable)
            {
                return false;
            }
            
            // Create a push notifications channel
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            channel.PushNotificationReceived += Channel_PushNotificationReceived;
            var hub = new NotificationHub("BlueYonderHub", "Endpoint=sb://blueyonderhub.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=1Z0s1slYMa8BkxtifP3vqystnRnnM5ERiYVtqK9Hmfw=");
            var result = await hub.RegisterNativeAsync(channel.Uri, new string[] {
                $"user-{userAuth.Traveler.TravelerId}"
            });
            return result.ChannelUri != null;
            // Encode the channel uri
            //var encodedChannelUri = EncodeChannelUri(channel.Uri);

            // Send the encoded channel uri to the server
            //var success = await SendChannelToServer(result.ChannelUri);
            //return success;
        }

        private void Channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            ToastNotificationManager.CreateToastNotifier().Show(args.ToastNotification);
        }
    }
}
