using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using BlueYonder.Companion.Client.Common;

namespace BlueYonder.Companion.Client.Helpers
{
    public class CacheManager
    {
        private static StorageFolder GetTemporaryFolder()
        {
            return ApplicationData.Current.TemporaryFolder;
        }

        public async static Task Save(CacheType cacheType, object data)
        {
            var tempFolder = GetTemporaryFolder();
            var file = await tempFolder.CreateFileAsync(cacheType.ToString(), CreationCollisionOption.ReplaceExisting);

            var cache = new CacheItem();
            cache.CreatedDate = DateTime.Now;
            cache.JsonData = JsonSerializerHelper.Serialize(data);

            var json = JsonSerializerHelper.Serialize(cache);

            await FileIO.WriteTextAsync(file, json);
        }

        public async static Task<CacheItem> Load<T>(CacheType cacheType)
        {
            var tempFolder = GetTemporaryFolder();
            var file = await tempFolder.CreateFileAsync(cacheType.ToString(), CreationCollisionOption.OpenIfExists);
            var fileContent = await FileIO.ReadTextAsync(file);
            if (string.IsNullOrEmpty(fileContent))
                return null;

            CacheItem cache = JsonSerializerHelper.Deserialize<CacheItem>(fileContent);
            cache.Data = JsonSerializerHelper.Deserialize<T>(cache.JsonData);
            return cache;
        }

        public static async void Invalidate(CacheType cacheType)
        {
            var tempFolder = GetTemporaryFolder();
            var file = await tempFolder.GetFileAsync(cacheType.ToString());
            if (file != null)
            {
                await file.DeleteAsync();
            }
        }
    }
}
