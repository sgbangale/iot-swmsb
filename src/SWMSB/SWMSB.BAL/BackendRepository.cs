using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SWMSB.COMMON;
using SWMSB.DEVICE;
using SWMSB.PROVIDERS;
using System;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace SWMSB.BAL
{
    public interface IBackendRepository
    {
        Task<IoTHubDeviceResultStatus> BackendMsgReceivedAsync(string msg);
        Task<bool> RegisterDeviceBackendRequest(DeviceRequest msg);
        Task<bool> SendLeakEmailAsync(Alert alert);
        Task<bool> SendUsageExhaustEmailAsync(Alert alert);


    }

    public sealed class BackendRepository : IBackendRepository
    {
        ILogger logger;

        public IotHubManagerRepository Manager { get; }
        public Config Config { get; set; }

        public BackendRepository(Config _config, ILogger _logger)
        {
            Config = _config;
            logger = _logger;
            Manager = new IotHubManagerRepository(Config, logger);
        }

        private async Task<IoTHubDeviceResultStatus> HandleRegisterDeviceRequestAsync(DeviceRequest payload)
        {
           
            var result = await Manager.AddDeviceAsync(payload.DeviceId);

            if (result == IoTHubDeviceResultStatus.DEVICE_CREATED)
            {
                await Manager.UpdateDeviceTwinAsync(new DeviceAttribute { DeviceId = payload.DeviceId, Etag = "AAAAAAAAAAI=" });
                var sendResult = await Manager.SendEventAsync(payload.TTNPayload);
                return sendResult;
            }
            else if (result == IoTHubDeviceResultStatus.DEVICE_FOUND)
            {
                await Manager.UpdateDeviceTwinAsync(new DeviceAttribute { DeviceId = payload.DeviceId, Etag = "AAAAAAAAAAI=" });
                return result;
            }
            else if (result == IoTHubDeviceResultStatus.INVALID_REQUEST)
            {
                return result;
            }
            else
            {
                return IoTHubDeviceResultStatus.INVALID_REQUEST;
            }
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
                    return await this.HandleRegisterDeviceRequestAsync(payload);
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
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error-{typeof(BackendRepository)}");
                return false;
            }
        }

        public async Task<bool> SendLeakEmailAsync(Alert alert)
        {

            try
            {
                IotHubManagerRepository iotHubManagerRepository = new IotHubManagerRepository(Config, logger);
                var data = await iotHubManagerRepository.GetDeviceTwinAsync(new DeviceAttribute { DeviceId = alert.DevId });
                EmailProvider emailProvider = new EmailProvider(Config, logger);
                var key = $"LEAK:{alert.DevId}";
                if (MemoryCache.Default.Contains(key))
                {
                    var emailresult = await emailProvider.SendEmail(
                               new EmailMsg
                               {
                                   EmailBody = $"Please check the water leak/tap closure in Apt no:{data.Appartment} ",
                                   Subject = "Smart Water Meter Leak Alert",
                                   ReceiverEmail = data.AppartmentOwnerEmail,
                                   ReceiverName = data.AppartmentOwnerEmail
                               });

                    if (emailresult)
                    {
                        MemoryCache.Default.Add(new CacheItem(key, alert.ToIntendedJsonString()), new CacheItemPolicy()
                        {
                            AbsoluteExpiration = DateTimeOffset.Now.AddHours(1)
                        });
                    }

                    return emailresult;
                }

                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error-{typeof(BackendRepository)}");
                return false;
            }
        }

        public async Task<bool> SendUsageExhaustEmailAsync(Alert alert)
        {

            try
            {
                IotHubManagerRepository iotHubManagerRepository = new IotHubManagerRepository(Config, logger);
                var data = await iotHubManagerRepository.GetDeviceTwinAsync(new DeviceAttribute { DeviceId = alert.DevId });
                EmailProvider emailProvider = new EmailProvider(Config, logger);
                var key = $"EXHAUST:{alert.DevId}";
                if (MemoryCache.Default.Contains(key))
                {
                    var emailresult = await emailProvider.SendEmail(
                             new EmailMsg
                             {
                                 EmailBody = $" Your consumption limit has reached the maximum limit for the day",
                                 Subject = $"Smart Water Usage Capacity Exhausted for {data.Appartment}",
                                 ReceiverEmail = data.AppartmentOwnerEmail,
                                 ReceiverName = data.AppartmentOwnerEmail
                             });
                    if (emailresult)
                    {
                        MemoryCache.Default.Add(new CacheItem(key, alert.ToIntendedJsonString()), new CacheItemPolicy()
                        {
                            AbsoluteExpiration = DateTimeOffset.Now.AddHours(24)
                        });
                    }

                    return emailresult;
                }

                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error-{typeof(BackendRepository)}");
                return false;
            }
        }
    }
}
