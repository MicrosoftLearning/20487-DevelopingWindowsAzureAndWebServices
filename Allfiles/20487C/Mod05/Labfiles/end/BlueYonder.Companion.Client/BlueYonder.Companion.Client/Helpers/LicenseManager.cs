using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO: Module 12: Exercise 1: Task 2.2: Add the required using statements
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;

namespace BlueYonder.Companion.Client.Helpers
{
    public class LicenseManager
    {

		private ListingInformation _listingInformation;
		
		//Singleton Implementations
        private static LicenseManager _instance;
        public static LicenseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LicenseManager();
                }
                return _instance;
            }
        }

		public bool IsTrialLicense { get; private set; }
        
		// TODO: Module 12: Exercise 1: Task 2.4: Add the LicenseDataUpdated event
        public event EventHandler LicenseDataUpdated;



        // TODO: Module 12: Exercise 2: Task 1.1: Add the readonly IsMediaFeatureEnabled property
        public bool IsMediaFeatureEnabled { get; private set; }

        
        private LicenseManager()
        {
            // TODO: Module 12: Exercise 1: Task 2.3: Subscribe to the LicenseChanged event
            CurrentAppSimulator.LicenseInformation.LicenseChanged += LicenseInformation_LicenseChanged;
        }


        private bool _loaded = false;
        public async Task LoadLicenseData()
        {
            if (_loaded)
                return;
			// TODO: Module 12: Exercise 1: Task 2.2: Load the license data from the data\license.xml file
            var installedFolder = await Package.Current.InstalledLocation.GetFolderAsync("data");
            var simulatorSettingsFile = await installedFolder.GetFileAsync("license.xml");
            await CurrentAppSimulator.ReloadSimulatorAsync(simulatorSettingsFile);

            // TODO: Module 12: Exercise 2: Task 1.2: Store the license information
            _listingInformation = await CurrentAppSimulator.LoadListingInformationAsync();

            _loaded = true;
        }

        // TODO: Module 12: Exercise 1: Task 2.3: Implement the LicenseChanged event handler
        private void LicenseInformation_LicenseChanged()
        {
            // TODO: Module 12: Exercise 1: Task 2.3: Only process the license information if the license is active
            if (!CurrentAppSimulator.LicenseInformation.IsActive)
                return;

            // TODO: Module 12: Exercise 1: Task 2.3: Determine whether the license is a trial license
            IsTrialLicense = false;// CurrentAppSimulator.LicenseInformation.IsTrial;

            // TODO: Module 12: Exercise 2: Task 1.1: Determine whether the media feature is enabled
            IsMediaFeatureEnabled = true;// CurrentAppSimulator.LicenseInformation.ProductLicenses["MediaFeature"].IsActive;

            // TODO: Module 12: Exercise 1: Task 2.4: Fire the LicenseDataUpdated event
            var handler = LicenseDataUpdated;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }


        public async Task PurchaseAppAsync()
        {
        // TODO: Module 12: Exercise 1: Task 3.1: Implement the PurchaseAppAsync method to purchase the app
            await LoadLicenseData();

            if (IsTrialLicense)
            {
                await CurrentAppSimulator.RequestAppPurchaseAsync(false);
            }
        }

        // TODO: Module 12: Exercise 2: Task 1.2: Implement the PurchaseMediaFeatureAsync method
        public async Task PurchaseMediaFeatureAsync()
        {
            var trialMessage = ResourceHelper.ResourceLoader.GetString("TrialMessage");
            var alreadyOwnMessage = ResourceHelper.ResourceLoader.GetString("YouAlreadyOwn");
            var boughtMessage = ResourceHelper.ResourceLoader.GetString("BoughtMessage");
            var unableToBuyMessage = ResourceHelper.ResourceLoader.GetString("UnableToBuy");
            var message = string.Empty;

            if (IsTrialLicense)
            {
                message = trialMessage;
            }
            else
            {
                var product = _listingInformation.ProductListings["MediaFeature"];
                if (IsMediaFeatureEnabled)
                {
                    message = string.Format("{0} '{1}'", alreadyOwnMessage, product.Name);
                }

                else
                {
                    try
                    {
                        await CurrentAppSimulator.RequestProductPurchaseAsync(product.ProductId, false);
                        message = string.Format("{0} '{1}'", boughtMessage, product.Name);
                    }
                    catch
                    {
                        message = string.Format("{0} '{1}'", unableToBuyMessage, product.Name);
                    }
                }
            }
            var msg = new Windows.UI.Popups.MessageDialog(message, "In-App Purchase");
            await msg.ShowAsync();
        }
    }
}
