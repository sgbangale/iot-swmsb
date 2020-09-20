using System;

namespace SWMSB.COMMON
{
    public class Config
    {
        public string TTN_API_KEY { get; }
        public string TTN_DATA_STORAGE_ENDPOINT { get; }
        public string BlobStorageConnectionString { get; }
        public string StorageTelemetryTableName { get; }
        public string IOT_HUB_CS { get; set; }
        public string BACKEND_REQUEST_ENTITY_PATH { get; set; }
        public string BACKEND_REQUEST_EVENT_HUB { get; }
        public string TTN_DOWNLINK_URL { get; set; }
        public string SENDGRID_API_KEY { get; set; }
        public Config()
        {  
            BlobStorageConnectionString = Environment.GetEnvironmentVariable(CommonConstants.BlobStorageConnectionString);
            StorageTelemetryTableName = Environment.GetEnvironmentVariable(CommonConstants.StorageTelemetryTableName);
            TTN_API_KEY = Environment.GetEnvironmentVariable(CommonConstants.TTN_API_KEY);
            TTN_DATA_STORAGE_ENDPOINT = Environment.GetEnvironmentVariable(CommonConstants.TTN_DATA_STORAGE_ENDPOINT);
            IOT_HUB_CS = Environment.GetEnvironmentVariable(CommonConstants.IOT_HUB_CS);
            BACKEND_REQUEST_ENTITY_PATH = Environment.GetEnvironmentVariable(CommonConstants.BACKEND_REQUEST_ENTITY_PATH);
            BACKEND_REQUEST_EVENT_HUB = Environment.GetEnvironmentVariable(CommonConstants.BACKEND_REQUEST_EVENT_HUB);
            TTN_DOWNLINK_URL = Environment.GetEnvironmentVariable(CommonConstants.TTN_DOWNLINK_URL);
            SENDGRID_API_KEY = Environment.GetEnvironmentVariable(CommonConstants.SENDGRID_API_KEY);
        }
    }
  
}
