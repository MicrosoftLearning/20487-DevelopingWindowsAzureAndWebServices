using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace BlueYonder.Companion.Client.DataModel
{
    public enum FolderType
    {
        Temp,
        Local
    }

    public class MediaItem
    {
        public string Name { get; private set; }
        public int ReservationId { get; set; }
        public FolderType FolderType { get; private set; }
        public string BasePath { get; set; }

        public MediaItem(FolderType folderType, int reservationId, string name)
        {
            FolderType = folderType;
            ReservationId = reservationId;
            Name = name;
            CreateImageFromFile();
        }

        public MediaItem(FolderType folderType, string basePath, string name)
        {
            FolderType = folderType;
            BasePath = basePath;
            Name = name;
            CreateImageFromFile();
        }

        public string Path
        {
            get
            {
                if (string.IsNullOrEmpty(BasePath))
                {
                    return string.Format("ms-appdata:///{0}/Media/{1}/{2}", FolderType, ReservationId, Name);
                }
                return string.Format("{0}/{1}", BasePath, Name);
            }
        }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get { return _image; }
        }

        private async void CreateImageFromFile()
        {
            try
            {
                _image = new BitmapImage();
                var uri = new Uri(Path);
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    _image.SetSource(fileStream);
                }
            }
            catch (Exception e)
            {
                // Could not create a BitmapImage from Source
            }
        }
    }
}
