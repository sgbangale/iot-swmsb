using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using SWMSB.COMMON;
using SWMSB.DEVICE;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SWMSB.DATA
{
    public class IoTHubDeviceClientProvider
    {
        private readonly string NOT_FOUND_IOT_DEVICE = "condition:amqp:not-found";
        public string IoTHubConnectionString { get; set; }
        public ILogger log { get; }
        public Config Config { get; }
        public DeviceClient DeviceClient { get; }

        public IoTHubDeviceClientProvider(Config config, string deviceid, ILogger _logger)
        {
            log = _logger;
            Config = config;
            DeviceClient = DeviceClient.CreateFromConnectionString(config.IOT_HUB_CS, deviceid);
        }
        public async Task<IoTHubDeviceResultStatus> SendEventAsync(Root ttnPayload)
        {
            int retryCount = 0;
            Random randomDelay = new Random();
          
            while (true)
            {
                try
                {
                    var data = new Message(Encoding.UTF8.GetBytes(ttnPayload.ToIntendedJsonString()))
                    {
                        ContentType = "application/json",
                        MessageId = Guid.NewGuid().ToString()
                    };
                    await DeviceClient.SendEventAsync(data);
                    return IoTHubDeviceResultStatus.MSG_SENT;
                }
                catch (Exception ex)
                {
                    log.LogError($"{ttnPayload.DevId}-{ex.Message}", ex, $"Error-{typeof(IoTHubDeviceClientProvider)}");
                    if (ex.Message.Contains(NOT_FOUND_IOT_DEVICE))
                    {
                        return IoTHubDeviceResultStatus.DEVICE_NOT_FOUND;

                    }
                    retryCount++;
                    if (retryCount > 5)
                        return IoTHubDeviceResultStatus.MSG_FAILED;
                    await Task.Delay(randomDelay.Next(1000, 2000));
                }
            }
        }

    }
}
