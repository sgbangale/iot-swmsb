using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SWMSB.COMMON;
using SWMSB.DEVICE;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SWMSB.BAL
{
    public interface IBackendRepository
    {
        Task<IoTHubDeviceResultStatus> BackendMsgReceivedAsync(string msg);
        Task<bool> RegisterDeviceBackendRequest(DeviceRequest msg);

    }

    public sealed class BackendRepository : IBackendRepository
    {
        ILogger logger;
        public Config Config { get; set; }

        public BackendRepository(Config _config, ILogger _logger)
        {
            Config = _config;
            logger = _logger;
        }

        public async Task<IoTHubDeviceResultStatus> BackendMsgReceivedAsync(string msg)
        {
            logger.LogInformation("BackendMsgReceived-start");
            var payload = JsonConvert.DeserializeObject<DeviceRequest>(msg);

            logger.LogInformation($"msg-body-{payload.ToIntendedJsonString()}");

            //validation of payload
            Validation payloadValidationResult = payload.Validate();
            if (!payloadValidationResult.Success)
            {
                logger.LogInformation(payloadValidationResult.ErrorMessage);
                return IoTHubDeviceResultStatus.INVALID_REQUEST;
            }

            switch (payload.Type)
            {
                case BackendRequestType.REGISTER_DEVICE_TO_IOTHUB:
                    IiotHubManagerRepository manager = new IotHubManagerRepository(Config, logger);
                    var result = await manager.AddDeviceAsync(payload.DeviceId);

                    if (result == IoTHubDeviceResultStatus.DEVICE_CREATED)
                    {
                        IotHubManagerRepository.AddOrUpdateToCache(payload.DeviceId);
                        IiotHubManagerRepository iiotHubManagerRepository = new IotHubManagerRepository(Config, logger);
                        var sendResult = await iiotHubManagerRepository.SendEventAsync(payload.TTNPayload);
                        return sendResult;
                    }
                    else if (result == IoTHubDeviceResultStatus.DEVICE_FOUND)
                    {
                        IotHubManagerRepository.AddOrUpdateToCache(payload.DeviceId);
                    }
                    else if (result == IoTHubDeviceResultStatus.INVALID_REQUEST)
                    {
                        return result;
                    }
                    break;
                case BackendRequestType.DELETE_DEVICE_IOTHUB:
                    break;
                case BackendRequestType.ENABLE_DEVICE:
                    break;
                case BackendRequestType.DISABLE_DEVICE:
                    break;
                default:
                    break;
            }

            logger.LogInformation("BackendMsgReceived-end");
            return IoTHubDeviceResultStatus.INVALID_REQUEST;
        }

        public async Task<bool> RegisterDeviceBackendRequest(DeviceRequest msg)
        {
            try
            {
                var connectionStringBuilder = new EventHubsConnectionStringBuilder(Config.BACKEND_REQUEST_EVENT_HUB)
                {
                    EntityPath = Config.BACKEND_REQUEST_ENTITY_PATH
                };

                var eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

                var eventMessage = JsonConvert.SerializeObject(msg);
                await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(eventMessage)));
                IotHubManagerRepository.AddOrUpdateToCache(msg.DeviceId);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error-{typeof(BackendRepository)}");
                return false;
            }
        }
    }
}
