using System;

namespace BlueYonder.Companion.Client.Helpers
{
    public class CacheItem
    {
        public DateTime CreatedDate { get; set; }
        public string JsonData { get; set; }
        public object Data { get; set; }

        public bool RequireRefresh
        {
            get { return this.CreatedDate < DateTime.Now.AddMinutes(-5); }
        }
    }
}