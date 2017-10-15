using System.Threading.Tasks;
using BlueYonder.Companion.Shared;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

namespace BlueYonder.Companion.Tasks
{
    public sealed class WeatherUpdateTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            WeatherForecast weather = await GetWeatherAsync();
            if (weather != null)
            {
                var condition = weather.Condition.ToString();
                var celsius = weather.TemperatureCelsius.ToString();
                var fahrenheit = weather.TemperatureFahrenheit.ToString();

                ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;
                settings.Values["weather.condition"] = condition;
                settings.Values["weather.celsius"] = celsius;
                settings.Values["weather.fahrenheit"] = fahrenheit;

                UpdateTile(condition, celsius, fahrenheit);
            }

            deferral.Complete();
        }

        private async Task<WeatherForecast> GetWeatherAsync()
        {
            int locationId;
            int.TryParse(GetSetting("locationId"), out locationId);
            DateTime departure;
            DateTime.TryParse(GetSetting("departure"), out departure);

            var uri = new Uri(string.Format(Addresses.GetWeatherUri, locationId, departure));
            var responseContent = string.Empty;
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(uri);
                responseContent = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("GetWeatherAsync", ex);
            }

            var weather = Deserialize(responseContent);
            return weather;
        }

        private static void UpdateTile(string condition, string celsius, string fahrenheit)
        {
            var tile = TileUpdateManager.CreateTileUpdaterForApplication();
            if (tile.Setting != NotificationSetting.Enabled)
                return;

            var locationName = GetSetting("locationName");

            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = locationName,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },
                                new AdaptiveText()
                                {
                                    Text = condition,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },
                                new AdaptiveText()
                                {
                                    Text = "Celsius: " + celsius,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },
                                new AdaptiveText()
                                {
                                    Text = "Fahrenheit: " + fahrenheit,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },
                            }
                        }
                    },

                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = locationName,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },
                                new AdaptiveText()
                                {
                                    Text = condition,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },
                                new AdaptiveText()
                                {
                                    Text = "Celsius: " + celsius,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },
                                new AdaptiveText()
                                {
                                    Text = "Fahrenheit: " + fahrenheit,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },
                            }
                        }
                    },
                    Branding = TileBranding.Logo
                }
            };
            //ITileWideText01 tileContent = TileContentFactory.CreateTileWideText01();
            //tileContent.TextHeading.Text = locationName;
            //tileContent.TextBody1.Text = condition;
            //tileContent.TextBody2.Text = "Celsius: " + celsius;
            //tileContent.TextBody3.Text = "Fahrenheit: " + fahrenheit;

            //ITileSquareBlock squareTileContent = TileContentFactory.CreateTileSquareBlock();
            //squareTileContent.TextBlock.Text = celsius;
            //squareTileContent.TextSubBlock.Text = locationName;
            //tileContent.SquareContent = squareTileContent;

            //tileContent.Branding = TileBranding.Logo;

            TileNotification tileNotification = new TileNotification(tileContent.GetXml());
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        public static WeatherForecast Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<WeatherForecast>(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Deserialize->{0}", ex.Message);
                return null;
            }
        }

        private static string GetSetting(string key)
        {
            var settings = ApplicationData.Current.LocalSettings;
            var value = settings.Values[key];
            return value == null ? null : value.ToString();
        }
    }
}
