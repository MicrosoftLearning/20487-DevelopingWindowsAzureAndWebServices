using BlueYonder.Companion.Client.Common;
using BlueYonder.Companion.Client.DataModel;
using BlueYonder.Companion.Client.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueYonder.Companion.Shared;

namespace BlueYonder.Companion.Client.ViewModels
{
    public class TravelerInfoViewModel : ViewModel
    {
        private readonly Settings _settings;

        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand ResetCommand { get; set; }

        private int _travelerId;
        public int TravelerId
        {
            get { return this._travelerId; }
            set { this.SetProperty(ref this._travelerId, value); }
        }

        private string _firstName;
        public string FirstName
        {
            get { return this._firstName; }
            set { this.SetProperty(ref this._firstName, value); }
        }

        private string _lastName;
        public string LastName
        {
            get { return this._lastName; }
            set { this.SetProperty(ref this._lastName, value); }
        }

        private string _passport;
        public string Passport
        {
            get { return this._passport; }
            set { this.SetProperty(ref this._passport, value); }
        }

        private string _mobilePhone;
        public string MobilePhone
        {
            get { return this._mobilePhone; }
            set { this.SetProperty(ref this._mobilePhone, value); }
        }

        private string _homeAddress;
        public string HomeAddress
        {
            get { return this._homeAddress; }
            set { this.SetProperty(ref this._homeAddress, value); }
        }

        private string _email;
        public string Email
        {
            get { return this._email; }
            set { this.SetProperty(ref this._email, value); }
        }

        private string _message;
        public string Message
        {
            get { return this._message; }
            private set { this.SetProperty(ref this._message, value); }
        }

        private string _frequentFlyerMessage;
        public string FrequentFlyerMessage
        {
            get { return this._frequentFlyerMessage; }
            private set { this.SetProperty(ref this._frequentFlyerMessage, value); }
        }

        public TravelerInfoViewModel()
        {
            _settings = new Settings(SettingsType.Local);
            LoadFromLocalSettings();

            SaveCommand = new DelegateCommand(Save);
            ResetCommand = new DelegateCommand(Reset);
        }

        private void LoadFromLocalSettings()
        {
            var travelerIdString = _settings.Get(Constants.TravelerId);
            int travelerId;
            int.TryParse(travelerIdString, out travelerId);

            var firstName = _settings.Get(Constants.FirstName);
            var lastName = _settings.Get(Constants.LastName);
            var passportNumber = _settings.Get(Constants.Passport);
            var mobileNumber = _settings.Get(Constants.MobilePhone);
            var homeAddress = _settings.Get(Constants.HomeAddress);
            var email = _settings.Get(Constants.Email);

            LoadTravelerInfo(travelerId, firstName, lastName, passportNumber, mobileNumber, homeAddress, email);

            if (travelerId > 0)
            {
                SetFrequentFlyerMiles(travelerId);
            }
        }

        private async void SetFrequentFlyerMiles(int travelerId)
        {
            var frequentFlyerProvider = new FrequentFlyerService.FrequentFlyerProvider(Addresses.GetFrequentFlyerMilesUri);
            var frequentFlyerMiles = await frequentFlyerProvider.GetFrequentFlyerMilesAsync(travelerId);
            this.FrequentFlyerMessage = string.Format(ResourceHelper.ResourceLoader.GetString("FrequentFlyerMiles"), frequentFlyerMiles);
        }

        private void Reset(object parameter)
        {
            LoadFromLocalSettings();
        }

        public void Save(object parameter)
        {
            StoreInLocalSettings();
            StoreOnServer();

            this.Message = ResourceHelper.ResourceLoader.GetString("TravelerInformationSaved");
        }

        private void StoreInLocalSettings()
        {
            _settings.Add(Constants.TravelerId, this.TravelerId.ToString());
            _settings.Add(Constants.FirstName, this.FirstName);
            _settings.Add(Constants.LastName, this.LastName);
            _settings.Add(Constants.Passport, this.Passport);
            _settings.Add(Constants.HomeAddress, this.HomeAddress);
            _settings.Add(Constants.MobilePhone, this.MobilePhone);
            _settings.Add(Constants.Email, this.Email);
        }

        private async void StoreOnServer()
        {
            var traveler = new Traveler()
            {
                TravelerId = this.TravelerId,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Passport = this.Passport,
                HomeAddress = this.HomeAddress,
                MobilePhone = this.MobilePhone,
                Email = this.Email
            };

            try
            {
                var data = new DataManager();
                await data.UpdateTravelerAsync(traveler);
            }
            catch
            {
                // The server could not be reached or the authentication failed
            }
        }

        private void LoadTravelerInfo(int travelerId, string firstName, string lastName, string passportNumber, string mobileNumber, string homeAddress, string email)
        {
            this.TravelerId = travelerId;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.MobilePhone = mobileNumber;
            this.HomeAddress = homeAddress;
            this.Passport = passportNumber;
            this.Email = email;
        }

        public bool IsValid()
        {
            return
                !string.IsNullOrEmpty(this.FirstName)
                    && !string.IsNullOrEmpty(this.LastName)
                    && !string.IsNullOrEmpty(this.MobilePhone)
                    && !string.IsNullOrEmpty(this.HomeAddress)
                    && !string.IsNullOrEmpty(this.Passport)
                    && !string.IsNullOrEmpty(this.Email);
        }
    }
}
