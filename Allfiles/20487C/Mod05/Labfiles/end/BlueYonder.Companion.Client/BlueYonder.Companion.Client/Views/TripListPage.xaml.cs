using BlueYonder.Companion.Client.Common;
using BlueYonder.Companion.Client.DataModel;
using BlueYonder.Companion.Client.Helpers;
using BlueYonder.Companion.Client.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace BlueYonder.Companion.Client.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class TripListPage : BlueYonder.Companion.Client.Common.LayoutAwarePage
    {
        public TripListPage()
        {
            this.InitializeComponent();

            this.DataContext = new TripListViewModel();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            var viewModel = (TripListViewModel)DataContext;
            viewModel.ForceRefresh = (bool) navigationParameter;
            viewModel.Initialize(this.Frame);
        }

        /// <summary>
        /// Invoked when a group header is clicked.
        /// </summary>
        /// <param name="sender">The Button used as a group header for the selected group.</param>
        /// <param name="e">Event data that describes how the click was initiated.</param>
        void Header_Click(object sender, RoutedEventArgs e)
        {
            // Determine what group the Button instance represents
            var trip = (TripCategory)((sender as FrameworkElement).DataContext);
            var argument = JsonSerializerHelper.Serialize(new TripDetailNavigationArgument()
            {
                CategoryType = trip.Type,
                ReservationId = null
            });
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            this.Frame.Navigate(typeof(TripDetailPage), argument);
        }

        /// <summary>
        /// Invoked when an item within a group is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var item = ((Reservation)e.ClickedItem);
            var argument = JsonSerializerHelper.Serialize(new TripDetailNavigationArgument()
            {
                CategoryType = item.Type,
                ReservationId = item.ReservationId
            });
            this.Frame.Navigate(typeof(TripDetailPage), argument);
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
        private void NavigateToSearchPage(string queryText)
        {
            var argument = JsonSerializerHelper.Serialize(new TripDetailNavigationArgument()
            {
                CategoryType = CategoryType.SearchResult,
                QueryText = queryText
            });

            var frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(TripDetailPage), argument);
            Window.Current.Content = frame;
        }

        private async void SearchBox_OnSuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs e)
        {
            //Request deferral must be recieved to allow async search suggestion population
            var deferal = e.Request.GetDeferral();

            // Add suggestions to Search Pane
            var destinations = await LocationsDataFetcher.Instance.FetchLocationsAsync(e.QueryText, false);
            var suggestions =
                destinations
                    .OrderBy(location => location.City)
                    .Take(5)
                    .Select(location => location.City);
            e.Request.SearchSuggestionCollection.AppendQuerySuggestions(suggestions);
            deferal.Complete();
        }

        private void SearchBox_OnQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            NavigateToSearchPage(args.QueryText);
        }

        private void Search_Clicked(object sender, RoutedEventArgs e)
        {
            LocationsSearchBox.Focus(FocusState.Pointer);
        }
    }
}
