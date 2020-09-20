using Microsoft.Extensions.Logging;
using SWMSB.COMMON;
using SWMSB.DEVICE;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace SWMSB.BAL
{
    public interface ITTNRepository
    {
        Task<IoTHubDeviceResultStatus> TelemetryMsgReceivedAsync(Root msg);
    }

    public sealed class TTNRepository : GenericRepository<Root>, ITTNRepository
    {
        ILogger logger;
        public Config Config { get; set; }
        public MemoryCache LocalCache { get; set; }
        public TTNRepository(Config _config, ILogger _logger, MemoryCache _localCache) : base(_config)
        {
            LocalCache = _localCache;
            Config = _config;
            logger = _logger;
        }

        public async Task<IoTHubDeviceResultStatus> TelemetryMsgReceivedAsync(Root ttnPayload)
        {
            logger.LogInformation("msg is at repo-start");
            logger.LogInformation($"msg-body-{ttnPayload.ToIntendedJsonString()}");

            IiotHubManagerRepository iotHubManagerRepository = new IotHubManagerRepository(Config,logger);

            if (LocalCache.Get(ttnPayload.DevId.Trim()) != null)
            {
                //dispatch to ioh hub as telemetry event for device
                var resultiot = await iotHubManagerRepository.SendEventAsync(ttnPayload);
                if (resultiot == IoTHubDeviceResultStatus.DEVICE_NOT_FOUND)
                {
                    await RegisterDeviceBackendRequest(ttnPayload);
                    return IoTHubDeviceResultStatus.DEVICE_NOT_FOUND;
                }
                return resultiot;
            }
            else
            {
                return await HandleDeviceRegistration(ttnPayload, iotHubManagerRepository);
            }

        }

        private async Task<IoTHubDeviceResultStatus> HandleDeviceRegistration(Root ttnPayload, IiotHubManagerRepository iotHubDeviceClientProvider)
        {
            var iothubManager = new IotHubManagerRepository(Config, logger);

            if (await iothubManager.IsDeviceRegisteredAsync(ttnPayload.DevId))
            {
                IotHubManagerRepository.AddOrUpdateToCache(ttnPayload.DevId);
                var resultiot = await iotHubDeviceClientProvider.SendEventAsync(ttnPayload);
                if (resultiot == IoTHubDeviceResultStatus.DEVICE_NOT_FOUND)
                {
                    await RegisterDeviceBackendRequest(ttnPayload);
                    return IoTHubDeviceResultStatus.DEVICE_NOT_FOUND;
                }
            }
            else
            {
                await RegisterDeviceBackendRequest(ttnPayload);
            }

            return IoTHubDeviceResultStatus.DEVICE_NOT_FOUND;
        }

        private async Task RegisterDeviceBackendRequest(Root ttnPayload)
        {
            BackendRepository backendRepository = new BackendRepository(Config, logger);
            await backendRepository.RegisterDeviceBackendRequest(new DeviceRequest
            {
                DeviceId = ttnPayload.DevId,
                TTNPayload = ttnPayload,
                Type = BackendRequestType.REGISTER_DEVICE_TO_IOTHUB
            });
        }
    }
}
