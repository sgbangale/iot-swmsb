using System;
using System.Collections.Generic;
using System.Text;

namespace SWMSB.COMMON
{
    public class Config
    {
        public string TTN_API_KEY { get; }
        public string TTN_DATA_STORAGE_ENDPOINT { get; }
        public DocumentDbSecret DocumentSecreteKeys { get; }
        public string BlobStorageConnectionString { get; }
        public string StorageTelemetryTableName { get; }
        public string IOT_HUB_CS { get; set; }

        public string BACKEND_REQUEST_ENTITY_PATH { get; set; }
        public string BACKEND_REQUEST_EVENT_HUB { get; }

        public Config()
        {
            DocumentSecreteKeys = new DocumentDbSecret();
            BlobStorageConnectionString = Environment.GetEnvironmentVariable(CommonConstants.BlobStorageConnectionString);
            StorageTelemetryTableName = Environment.GetEnvironmentVariable(CommonConstants.StorageTelemetryTableName);
            TTN_API_KEY = Environment.GetEnvironmentVariable(CommonConstants.TTN_API_KEY);
            TTN_DATA_STORAGE_ENDPOINT = Environment.GetEnvironmentVariable(CommonConstants.TTN_DATA_STORAGE_ENDPOINT);
            IOT_HUB_CS = Environment.GetEnvironmentVariable(CommonConstants.IOT_HUB_CS);
            BACKEND_REQUEST_ENTITY_PATH = Environment.GetEnvironmentVariable(CommonConstants.BACKEND_REQUEST_ENTITY_PATH);
            BACKEND_REQUEST_EVENT_HUB = Environment.GetEnvironmentVariable(CommonConstants.BACKEND_REQUEST_EVENT_HUB);
        }

    }
    public class DocumentDbSecret
    {
        public DocumentDbSecret()
        {
            DocumentDbEndpointUrl = Environment.GetEnvironmentVariable(CommonConstants.DocumentDbEndpointUrl);
            DocumentDbAuthorizationKey = Environment.GetEnvironmentVariable(CommonConstants.DocumentDbAuthorizationKey);
            DocumentDbName = Environment.GetEnvironmentVariable(CommonConstants.DocumentDbName);
            DocumentCollectionName = Environment.GetEnvironmentVariable(CommonConstants.DocumentCollectionName);
        }

        public string DocumentDbEndpointUrl { get; }
        public string DocumentDbAuthorizationKey { get; }
        public string DocumentDbName { get; }
        public string DocumentCollectionName { get; }
    }
}
