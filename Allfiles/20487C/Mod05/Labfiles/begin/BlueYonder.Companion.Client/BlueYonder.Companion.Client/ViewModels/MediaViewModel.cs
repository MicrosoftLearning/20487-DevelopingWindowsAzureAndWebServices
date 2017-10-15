using System.IO;
using BlueYonder.Companion.Client.Common;
using BlueYonder.Companion.Client.DataModel;
using BlueYonder.Companion.Client.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace BlueYonder.Companion.Client.ViewModels
{
    public class MediaViewModel : ViewModel
    {
        private readonly DataManager _data;

        public DelegateCommand CaptureVideoCommand { get; set; }
        public DelegateCommand CaptureImageCommand { get; set; }
        public DelegateCommand TakePictureCommand { get; set; }
        public DelegateCommand AddFilesFromDiskCommand { get; set; }

        public DelegateCommand UploadPublicMediaCommand { get; set; }
        public DelegateCommand UploadPrivateMediaCommand { get; set; }

        private Reservation _reservation;
        public Reservation Reservation
        {
            get { return this._reservation; }
            set { this.SetProperty(ref this._reservation, value); }
        }

        private ICollection<MediaItem> _mediaItems;
        public ICollection<MediaItem> MediaItems
        {
            get { return this._mediaItems; }
            set { this.SetProperty(ref this._mediaItems, value); }
        }

        private MediaCapture _mediaCapture;

        #region Contrast

        private double _contrastValue = 50;
        public double ContrastValue
        {
            get { return this._contrastValue; }
            set
            {
                this.SetProperty(ref this._contrastValue, value);
                SetContrastValue();
            }
        }

        private double _contrastMinValue = 0;
        public double ContrastMinValue
        {
            get { return this._contrastMinValue; }
            set { this.SetProperty(ref this._contrastMinValue, value); }
        }

        private double _contrastStepFrequency = 5;
        public double ContrastStepFrequency
        {
            get { return this._contrastStepFrequency; }
            set { this.SetProperty(ref this._contrastStepFrequency, value); }
        }

        private double _contrastMaxValue = 100;
        public double ContrastMaxValue
        {
            get { return this._contrastMaxValue; }
            set { this.SetProperty(ref this._contrastMaxValue, value); }
        }

        #endregion

        #region Brightness

        private double _brightnessMinValue = 0;
        public double BrightnessMinValue
        {
            get { return this._brightnessMinValue; }
            set { this.SetProperty(ref this._brightnessMinValue, value); }
        }

        private double _brightnessStepFrequency = 5;
        public double BrightnessStepFrequency
        {
            get { return this._brightnessStepFrequency; }
            set { this.SetProperty(ref this._brightnessStepFrequency, value); }
        }

        private double _brightnessMaxValue = 100;
        public double BrightnessMaxValue
        {
            get { return this._brightnessMaxValue; }
            set { this.SetProperty(ref this._brightnessMaxValue, value); }
        }

        private double _brightnessValue = 50;
        public double BrightnessValue
        {
            get { return this._brightnessValue; }
            set
            {
                this.SetProperty(ref this._brightnessValue, value);
                SetBrightnessValue();
            }
        }

        #endregion

        private bool _previewEnabled;
        public bool PreviewEnabled
        {
            get { return this._previewEnabled; }
            set { this.SetProperty(ref this._previewEnabled, value); }
        }

        private CaptureElement _previewElement;
        public CaptureElement PreviewElement
        {
            get { return this._previewElement; }
            set { this.SetProperty(ref this._previewElement, value); }
        }

        private bool _idle;
        public bool Idle
        {
            get { return this._idle; }
            set { this.SetProperty(ref this._idle, value); }
        }

        private int _reservationId;
        public int ReservationId
        {
            get { return this._reservationId; }
            set { this.SetProperty(ref this._reservationId, value); }
        }

        private TravelerInfoViewModel _travelerInfo;
        public TravelerInfoViewModel TravelerInfo
        {
            get { return this._travelerInfo; }
            set { this.SetProperty(ref this._travelerInfo, value); }
        }

        private string _message;
        public string Message
        {
            get { return this._message; }
            set { this.SetProperty(ref this._message, value); }
        }

        private StorageFolder _localMediaFolder;
        private StorageFolder _tempMediaFolder;

        private MediaItem _selectedMediaItem;
        public MediaItem SelectedMediaItem
        {
            get { return this._selectedMediaItem; }
            set { this.SetProperty(ref this._selectedMediaItem, value); }
        }

        private readonly TransferManager _transferManager;

        private int _operationsCount;

        private bool _isProgressVisible;
        public bool IsProgressVisible
        {
            get { return this._isProgressVisible; }
            set { this.SetProperty(ref this._isProgressVisible, value); }
        }

        public MediaViewModel()
        {
            this._data = new DataManager();

            this.TravelerInfo = new TravelerInfoViewModel();

            this.Idle = true;

            this.MediaItems = new ObservableCollection<MediaItem>();

            CaptureVideoCommand = new DelegateCommand(CaptureVideo);
            CaptureImageCommand = new DelegateCommand(CaptureImage);
            TakePictureCommand = new DelegateCommand(TakePicture);
            AddFilesFromDiskCommand = new DelegateCommand(AddFilesFromDisk);

            UploadPublicMediaCommand = new DelegateCommand(UploadPublicMedia);
            UploadPrivateMediaCommand = new DelegateCommand(UploadPrivateMedia);

            _transferManager = new TransferManager();
        }

        public override async void Initialize(Frame frame)
        {
            base.Initialize(frame);

            this.Reservation = await _data.GetReservationAsync(this.ReservationId);

            _localMediaFolder = await GetOrCreateFolder(ApplicationData.Current.LocalFolder, "Media", ReservationId.ToString());
            _tempMediaFolder = await GetOrCreateFolder(ApplicationData.Current.TemporaryFolder, "Media", ReservationId.ToString());

            EmptyTempFolder();

            await LoadMediaFiles();
        }

        private async Task<StorageFolder> GetOrCreateFolder(StorageFolder rootFolder, params string[] folderNames)
        {
            StorageFolder folder = rootFolder;
            foreach (var folderName in folderNames)
            {
                folder = await folder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
            }
            return folder;
        }

        private void SetBrightnessValue()
        {
            try
            {
                var success = _mediaCapture.VideoDeviceController.Brightness.TrySetValue(BrightnessValue);
                Message = success ? string.Empty : "Unable to change the brightness";
            }
            catch (Exception exception)
            {
                ShowFatalErrorMessageDialog(exception.Message, string.Empty);
            }
        }

        private void SetContrastValue()
        {
            try
            {
                var success = _mediaCapture.VideoDeviceController.Contrast.TrySetValue(ContrastValue);
                Message = success ? string.Empty : "Unable to change the contrast";
            }
            catch (Exception exception)
            {
                ShowFatalErrorMessageDialog(exception.Message, string.Empty);
            }
        }

        private void SetupVideoPreviewSettings()
        {
            var videoDeviceController = _mediaCapture.VideoDeviceController;
            if (videoDeviceController.Brightness != null && videoDeviceController.Brightness.Capabilities.Supported)
            {
                var brightness = videoDeviceController.Brightness;
                BrightnessMaxValue = brightness.Capabilities.Max;
                BrightnessMinValue = brightness.Capabilities.Min;
                BrightnessStepFrequency = brightness.Capabilities.Step;

                double controlValue;
                if (brightness.TryGetValue(out controlValue))
                {
                    BrightnessValue = controlValue;
                }
            }

            if (videoDeviceController.Contrast != null && videoDeviceController.Contrast.Capabilities.Supported)
            {
                var contrast = videoDeviceController.Contrast;
                ContrastMaxValue = contrast.Capabilities.Max;
                ContrastMinValue = contrast.Capabilities.Min;
                ContrastStepFrequency = contrast.Capabilities.Step;

                double controlValue;
                if (contrast.TryGetValue(out controlValue))
                {
                    ContrastValue = controlValue;
                }
            }
        }

        private async void CaptureImage(object parameter)
        {
            if (!await VerifyCameraAvailable())
                return;

            try
            {
                Idle = true;
                PreviewEnabled = true;

                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync();
                _mediaCapture.Failed += MediaCapture_Failed;

                PreviewElement.Source = _mediaCapture;

                await _mediaCapture.StartPreviewAsync();

                PreviewEnabled = true;
                Idle = true;

                SetupVideoPreviewSettings();
            }
            catch (Exception ex)
            {
                ShowFatalErrorMessageDialog(ex.Message, string.Empty);
            }
        }

        private async void TakePicture(object parameter)
        {
            Idle = false;

            await _mediaCapture.StopPreviewAsync();

            var fileName = string.Format("Photo-{0}.png", Guid.NewGuid());
            var file = await _localMediaFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

            var imageEncodingProperties = ImageEncodingProperties.CreatePng();
            await _mediaCapture.CapturePhotoToStorageFileAsync(imageEncodingProperties, file);

            var mediaItem = new MediaItem(FolderType.Local, ReservationId, file.Name);
            MediaItems.Add(mediaItem);
            SelectedMediaItem = mediaItem;

            PreviewEnabled = false;
            Idle = true;
        }

        private async void AddFilesFromDisk(object parameter)
        {
            this.Idle = false;

            var fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".jpeg");
            fileOpenPicker.FileTypeFilter.Add(".mp4");
            fileOpenPicker.FileTypeFilter.Add(".avi");
            fileOpenPicker.FileTypeFilter.Add(".mpg");
            var selectedFiles = await fileOpenPicker.PickMultipleFilesAsync();

            var alreadySetSelectedMediaItem = false;

            foreach (var selectedFile in selectedFiles)
            {
                var fileName = string.Format("{0}-{1}{2}", Path.GetFileNameWithoutExtension(selectedFile.Name), Guid.NewGuid(), Path.GetExtension(selectedFile.Name));
                var localFile = await selectedFile.CopyAsync(_localMediaFolder, fileName);

                var mediaItem = new MediaItem(FolderType.Local, ReservationId, localFile.Name);
                MediaItems.Add(mediaItem);

                if (!alreadySetSelectedMediaItem)
                {
                    SelectedMediaItem = mediaItem;
                    alreadySetSelectedMediaItem = true;
                }
            }

            this.Idle = true;
        }

        private void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            ShowFatalErrorMessageDialog(errorEventArgs.Message, string.Empty);
        }

        private async void CaptureVideo(object parameter)
        {
            if (!await VerifyCameraAvailable())
                return;

            try
            {
                CameraCaptureUI dialog = new CameraCaptureUI();
                dialog.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;

                StorageFile file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Video);
                if (file != null)
                {
                    var fileName = string.Format("Video-{0}.mp4", Guid.NewGuid());
                    await file.MoveAsync(_localMediaFolder, fileName, NameCollisionOption.GenerateUniqueName);

                    var mediaItem = new MediaItem(FolderType.Local, ReservationId, file.Name);
                    MediaItems.Add(mediaItem);
                    SelectedMediaItem = mediaItem;
                }
            }
            catch (Exception ex)
            {
                ShowFatalErrorMessageDialog(ResourceHelper.ResourceLoader.GetString("CaptureVideo"), ResourceHelper.ResourceLoader.GetString("CaptureVideoError"));
            }
        }

        private async Task LoadMediaFiles()
        {
            MediaItems.Clear();

            var localFiles = await _localMediaFolder.GetFilesAsync();
            var localMediaItems = localFiles.Select(file => new MediaItem(FolderType.Local, ReservationId, file.Name));
            foreach (var mediaItem in localMediaItems)
            {
                MediaItems.Add(mediaItem);
            }

            UpdateOperationsCount(+1);

            var files = await _data.GetAzureStorageFilesByTrip(ReservationId);
            foreach (var fileMetadata in files)
            {
                StorageFile destinationFile = await _tempMediaFolder.CreateFileAsync(fileMetadata.FileName, CreationCollisionOption.ReplaceExisting);

                await _transferManager.DownloadAsync(fileMetadata.Uri, destinationFile);

                var mediaItem = new MediaItem(FolderType.Temp, ReservationId, fileMetadata.FileName);
                MediaItems.Add(mediaItem);
            }

            UpdateOperationsCount(-1);
        }

        private static void ShowFatalErrorMessageDialog(string message, string title)
        {
            var msg = new MessageDialog("Fatal Error: " + message, title);
            msg.ShowAsync();
        }

        private async void UploadPublicMedia(object parameter)
        {
            await UploadMedia(false);
        }

        private async void UploadPrivateMedia(object parameter)
        {
            await UploadMedia(true);
        }

        private async Task UploadMedia(bool isPrivate)
        {
            if (SelectedMediaItem == null)
            {
                var msg = new MessageDialog(ResourceHelper.ResourceLoader.GetString("SelectMediaFileToUpload"));
                await msg.ShowAsync();
                return;
            }

            if (SelectedMediaItem.FolderType == FolderType.Temp)
            {
                var msg = new MessageDialog(ResourceHelper.ResourceLoader.GetString("MediaItemHasAlreadyBeenUploaded"));
                await msg.ShowAsync();
                return;
            }

            UpdateOperationsCount(+1);

            var fileName = SelectedMediaItem.Name;
            MediaItems.Remove(SelectedMediaItem);
            SelectedMediaItem = null;

            var locationId = Reservation.DepartureFlight.FlightInfo.Flight.Destination.LocationId;

            var file = await _localMediaFolder.GetFileAsync(fileName);

            await _transferManager.UploadAsync(file, TravelerInfo.TravelerId, ReservationId, isPrivate, locationId);

            try
            {
                await file.MoveAsync(_tempMediaFolder, file.Name, NameCollisionOption.ReplaceExisting);
            }
            catch (Exception exc)
            {
                ShowFatalErrorMessageDialog(exc.Message, string.Empty);
            }
            var mediaItem = new MediaItem(FolderType.Temp, ReservationId, file.Name);
            MediaItems.Add(mediaItem);
            SelectedMediaItem = mediaItem;

            UpdateOperationsCount(-1);
        }

        private async Task<bool> VerifyCameraAvailable()
        {
            var videoCaptureDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            var isCameraAvailable = videoCaptureDevices.Count > 0;
            if (isCameraAvailable)
            {
                var dialog = new MessageDialog("This operation requires a camera, but no camera could be found on this machine.", "This machine does not have a camera");
                await dialog.ShowAsync();
            }
            return isCameraAvailable;
        }

        private void UpdateOperationsCount(int increment)
        {
            _operationsCount += increment;
            _operationsCount = Math.Max(_operationsCount, 0);

            IsProgressVisible = _operationsCount > 0;
        }

        private async void EmptyTempFolder()
        {
            var files = await _tempMediaFolder.GetFilesAsync();
            foreach (var file in files)
            {
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }
    }
}
