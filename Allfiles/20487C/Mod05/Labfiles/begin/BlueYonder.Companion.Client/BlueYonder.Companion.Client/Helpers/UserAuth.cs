using System;
using System.Threading.Tasks;
using BlueYonder.Companion.Client.DataModel;

namespace BlueYonder.Companion.Client.Helpers
{
    public class UserAuth
    {
        private readonly DataManager _data;
        private readonly Settings _settings;

        public bool IsLoggedIn
        {
            get { return Traveler != null; }
        }

        public Traveler Traveler { get; private set; }

        private UserAuth()
        {
            _settings = new Settings(SettingsType.Local);
            _data = new DataManager();
        }

        public async Task<LoginResult> Login()
        {
            if (IsLoggedIn)
                return new LoginResult(true);

            try
            {
                await VerifyConnectionExists();

                await LoadTraveler();

                return new LoginResult(IsLoggedIn);
            }
            catch (Exception ex)
            {
                return new LoginResult(false, ex);
            }
        }

        public void Logout()
        {
            Traveler = null;
            _settings.Remove(Constants.TravelerId);
        }

        private async Task VerifyConnectionExists()
        {
            var errorMessage = ResourceHelper.ResourceLoader.GetString("LoginErrorNoInternetConnection");

            var isConnected = await NetworkManager.CheckInternetConnection(true, errorMessage);

            if(!isConnected)
            {
                throw new InvalidOperationException(errorMessage);
            }
        }

        private async Task LoadTraveler()
        {
            _settings.Remove(Constants.TravelerId);

            Traveler = await _data.GetTravelerAsync() ?? await _data.CreateTravelerAsync();
            if (Traveler != null)
            {
                _settings.Add(Constants.TravelerId, Traveler.TravelerId.ToString());
            }
        }

        private static UserAuth _instance;
        public static UserAuth Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserAuth();
                }
                return _instance;
            }
        }
    }
}
