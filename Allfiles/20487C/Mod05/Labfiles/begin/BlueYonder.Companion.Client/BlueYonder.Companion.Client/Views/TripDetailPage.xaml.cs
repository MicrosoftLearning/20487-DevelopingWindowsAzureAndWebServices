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
    /// The trip detail page shows trips by category and by query results.
    /// </summary>
    public sealed partial class TripDetailPage : BlueYonder.Companion.Client.Common.LayoutAwarePage
    {
        public TripDetailPage()
        {
            this.InitializeComponent();

            this.DataContext = new TripDetailViewModel();
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
            var viewModel = (TripDetailViewModel)this.DataContext;

            var json = (string)navigationParameter;
            var argument = JsonSerializerHelper.Deserialize<TripDetailNavigationArgument>(json);
            viewModel.CategoryType = argument.CategoryType;
            viewModel.ReservationId = argument.ReservationId;
            viewModel.SearchQuery = argument.QueryText;

            viewModel.Page = this.pageRoot;

            viewModel.Initialize(this.Frame);
        }

        /// <summary>
        /// Invoked when an item is left-clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var reservation = ((Reservation)e.ClickedItem);
            if (reservation != null)
            {
                if (itemListView.SelectedItem != reservation)
                {
                    itemListView.SelectedItem = reservation;
                }
                else
                {
                    itemListView.SelectedItem = null;
                }
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            var viewModel = (TripDetailViewModel)this.DataContext;
            viewModel.Uninitialize();
        }
    }
}
