using BlueYonder.Companion.Client.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace BlueYonder.Companion.Client.Helpers
{
    public class TileManager
    {
        public static Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }

        async public static Task<bool> Pin(object sender, Reservation reservation)
        {
            // Determine whether to pin or unpin.
            if (SecondaryTile.Exists(reservation.ReservationId.ToString()))
            {
                SecondaryTile secondaryTile = new SecondaryTile(reservation.ReservationId.ToString());
                Rect pinButtonRect = GetElementRect((FrameworkElement)sender);
                return await secondaryTile.RequestDeleteForSelectionAsync(pinButtonRect, Windows.UI.Popups.Placement.Above);
            }
            else
            {
                Uri logo = null;// GetPreviewImage(item, false);
                Uri logoWide = null;// GetPreviewImage(item, true);
                var flight = reservation.DepartureFlight.FlightInfo.Flight;
                var localizedSource = ResourceHelper.ResourceLoader.GetString("City_" + flight.Source.LocationId);
                var localizedDesintation = ResourceHelper.ResourceLoader.GetString("City_" + flight.Destination.LocationId);
                var title = string.Format(ResourceHelper.ResourceLoader.GetString("SourceToDestinationAtDate"), localizedSource, localizedDesintation, reservation.ReservationDate);
                SecondaryTile secondaryTile = new SecondaryTile(title, title, title, title, TileOptions.None, logo, logoWide);

                Rect pinButtonRect = GetElementRect((FrameworkElement)sender);
                return await secondaryTile.RequestCreateForSelectionAsync(pinButtonRect, Windows.UI.Popups.Placement.Above);
            }
        }
    }
}
