namespace SWMSB.COMMON
{
    public enum IoTHubDeviceResultStatus
    {
        DEVICE_NOT_FOUND,
        MSG_SENT,
        MSG_FAILED,
        DEVICE_CREATED,
        DEVICE_FOUND,
        INVALID_REQUEST

    }
    public class ApplicationConstants
    {

    }
    public class CommonConstants
    {
        public const string DocumentDbEndpointUrl = "DocumentDbEndpointUrl";
        public const string DocumentDbAuthorizationKey = "DocumentDbAuthorizationKey";
        public const string DocumentDbName = "DocumentDbName";
        public const string DocumentCollectionName = "DocumentCollectionName";
        public const string BlobStorageConnectionString = "BlobStorageConnectionString";
        public const string StorageTelemetryTableName = "StorageTelemetryTableName";
        public const string TTN_API_KEY = "TTN_API_KEY";
        public const string TTN_DATA_STORAGE_ENDPOINT = "TTN_DATA_STORAGE_ENDPOINT";
        public const string IOT_HUB_CS = "IOT_HUB_CS";
        public const string BACKEND_REQUEST_ENTITY_PATH = "BACKEND_REQUEST_ENTITY_PATH";
        public const string BACKEND_REQUEST_EVENT_HUB = "EventHubEndPoint";
    }
}
