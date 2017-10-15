using System;
using Windows.UI.Xaml.Data;
using BlueYonder.Companion.Client.Helpers;

namespace BlueYonder.Companion.Client.Common
{
    /// <summary>
    /// Value converter that determines the title of the detail page based on the type of data displayed on the page.
    /// </summary>
    public sealed class DetailPageTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is CategoryType)
            {
                var type = (CategoryType)value;
                switch (type)
                {
                    case CategoryType.UpcomingTrips: return ResourceHelper.ResourceLoader.GetString("UpcomingTrips");
                    case CategoryType.HistoricTrips: return ResourceHelper.ResourceLoader.GetString("HistoricTrips");
                    case CategoryType.SearchResult: return ResourceHelper.ResourceLoader.GetString("SearchResult");
                    default: return ResourceHelper.ResourceLoader.GetString("CurrentTrip");
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
