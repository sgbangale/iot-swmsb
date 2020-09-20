using Microsoft.Extensions.Logging;
using SWMSB.COMMON;
using SWMSB.DEVICE;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace SWMSB.BAL
{
    public interface ITTNRepository
    {
        Task<IoTHubDeviceResultStatus> TelemetryMsgReceivedAsync(TTNUpLinkPayload msg);
    }

    public sealed class TTNRepository : ITTNRepository
    {
        ILogger logger;

        public IIotHubManagerRepository Manager { get; }
        public Config Config { get; set; }
        public MemoryCache LocalCache { get; set; }
        public TTNRepository(Config _config, ILogger _logger, MemoryCache _localCache)
        {
            LocalCache = _localCache;
            Config = _config;
            logger = _logger;
            Manager = new IotHubManagerRepository(Config, logger);
        }

        public async Task<IoTHubDeviceResultStatus> TelemetryMsgReceivedAsync(TTNUpLinkPayload ttnPayload)
        {
            logger.LogInformation("msg is at repo-start");
            logger.LogInformation($"msg-body-{ttnPayload.ToIntendedJsonString()}");




            if (LocalCache.Get(ttnPayload.DevId.Trim()) != null)
            {
                //dispatch to ioh hub as telemetry event for device
                var resultiot = await Manager.SendEventAsync(ttnPayload);
                if (resultiot == IoTHubDeviceResultStatus.DEVICE_NOT_FOUND)
                {
                    await RegisterDeviceBackendRequest(ttnPayload);
                    return IoTHubDeviceResultStatus.DEVICE_NOT_FOUND;
                }
                return resultiot;
            }
            else
            {
                return await HandleDeviceRegistration(ttnPayload, Manager);
            }
        }

        private async Task<IoTHubDeviceResultStatus> HandleDeviceRegistration(TTNUpLinkPayload ttnPayload, IIotHubManagerRepository iotHubDeviceClientProvider)
        {
            if (await Manager.IsDeviceRegisteredAsync(ttnPayload.DevId))
            {
                var updateTwinData = await Manager.GetDeviceTwinAsync(new DeviceAttribute { DeviceId = ttnPayload.DevId });
                await Manager.UpdateDeviceTwinAsync(updateTwinData);
                var resultiot = await iotHubDeviceClientProvider.SendEventAsync(ttnPayload);
                if (resultiot == IoTHubDeviceResultStatus.DEVICE_NOT_FOUND)
                {
                    await RegisterDeviceBackendRequest(ttnPayload);
                    return IoTHubDeviceResultStatus.DEVICE_NOT_FOUND;
                }
                else
                {
                    return resultiot;
                }
            }
            else
            {
                await RegisterDeviceBackendRequest(ttnPayload);
            }

            return IoTHubDeviceResultStatus.DEVICE_NOT_FOUND;
        }

        private async Task RegisterDeviceBackendRequest(TTNUpLinkPayload ttnPayload)
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
