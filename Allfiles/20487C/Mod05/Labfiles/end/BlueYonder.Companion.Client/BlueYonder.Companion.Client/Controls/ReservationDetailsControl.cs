using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls.Primitives;
using BlueYonder.Companion.Client.DataModel;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace BlueYonder.Companion.Client.Controls
{
    [TemplatePart(Name = PART_CheckInButton, Type = typeof(ButtonBase))]
    public sealed class ReservationDetailsControl : Control
    {
        public const string PART_CheckInButton = "PART_CheckInButton";

        private ButtonBase _checkInButton;

        public ReservationDetailsControl()
        {
            this.DefaultStyleKey = typeof(ReservationDetailsControl);
        }

        public static readonly DependencyProperty ReservationProperty =
            DependencyProperty.Register("Reservation", typeof (Reservation), typeof (ReservationDetailsControl), null);

        public Reservation Reservation
        {
            get { return (Reservation) GetValue(ReservationProperty); }
            set { SetValue(ReservationProperty, value); }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_checkInButton != null)
                _checkInButton.Click -= OnCheckInButtonClick;

            _checkInButton = (ButtonBase) GetTemplateChild(PART_CheckInButton);

            if (_checkInButton != null)
                _checkInButton.Click += OnCheckInButtonClick;
        }

        private void OnCheckInButtonClick(object sender, RoutedEventArgs e)
        {
            if (Reservation == null)
                return;

            Reservation.IsCheckedIn = !Reservation.IsCheckedIn;
        }
    }
}
