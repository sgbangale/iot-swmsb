using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using SWMSB.COMMON;
using System;
using System.Threading.Tasks;

namespace SWMSB.PROVIDERS
{
    public class IoTHubManager
    {
        public string IoTHubConnectionString { get; set; }
        public ILogger log { get; }
        public RegistryManager Manager { get; }

        public IoTHubManager(string iothubcs, ILogger _logger)
        {
            log = _logger;
            Manager = RegistryManager.CreateFromConnectionString(iothubcs);
        }
        public async Task<bool> IsDeviceRegisteredAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return false;
            }
            Device iotDevice = await Manager.GetDeviceAsync(deviceId);
            return iotDevice != null;
        }
        public async Task<IoTHubDeviceResultStatus> AddDeviceAsync(string deviceId)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId))
                {
                    return IoTHubDeviceResultStatus.INVALID_REQUEST;
                }
                Device iotDevice = await Manager.GetDeviceAsync(deviceId);
                if (iotDevice == null)
                {
                    log.LogInformation($"device not found in iothub: {deviceId}");
                    var device = new Device(deviceId);

                    Device createdDevice = await Manager.AddDeviceAsync(device);
                    log.LogInformation($"device created-{createdDevice.Id}");
                    return IoTHubDeviceResultStatus.DEVICE_CREATED;
                }
                else
                {

                    log.LogInformation($"device found-{iotDevice.Id}");
                    return IoTHubDeviceResultStatus.DEVICE_FOUND;

                }
            }
            catch (Exception ex)
            {
                log.LogError($"{deviceId}-{ex.Message}", ex, $"Error-{typeof(IoTHubManager)}");
                return IoTHubDeviceResultStatus.MSG_FAILED;
            }
        }




    }
}
