using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SWMSB.COMMON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace SWMSB.PROVIDERS
{
    public class IoTHubProvider
    {
        public string IoTHubConnectionString { get; set; }
        public Config Config { get; }
        public ILogger log { get; }
        public RegistryManager Manager { get; }
        public IoTHubProvider(Config config, ILogger _logger)
        {
            Config = config;
            log = _logger;
            Manager = RegistryManager.CreateFromConnectionString(config.IOT_HUB_CS);
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
                log.LogError($"{deviceId}-{ex.Message}", ex, $"Error-{typeof(IoTHubProvider)}");
                return IoTHubDeviceResultStatus.MSG_FAILED;
            }
        }
        public async Task<DeviceAttribute> UpdateDeviceTwinAsync(DeviceAttribute deviceAttribute)
        {
            try
            {
                var twinGet = await GetDeviceTwin(deviceAttribute.DeviceId);
                var patch = @"{ tags: { apptno: '" + deviceAttribute.Appartment + "',email:'" + deviceAttribute.AppartmentOwnerEmail + "'}}";

                var twin = await Manager.UpdateTwinAsync(deviceAttribute.DeviceId, patch, twinGet.Etag);

                deviceAttribute.Etag = twin.ETag;

                return deviceAttribute;
            }
            catch (Exception ex)
            {
                log.LogError($"{deviceAttribute.ToIntendedJsonString()}-{ex.Message}", ex, $"Error-{typeof(IoTDeviceProvider)}");
                return null;
            }
        }
        public async Task<DeviceAttribute> GetDeviceTwin(string deviceId)
        {
            int retryCount = 0;
            Random randomDelay = new Random();

            while (true)
            {
                try
                {
                    var deviceAttr = new DeviceAttribute();
                    var twin = await Manager.GetTwinAsync(deviceId);
                    if (twin?.Tags?.Contains("apptno") ?? false)
                    {
                        deviceAttr.Appartment = twin?.Tags?["apptno"].ToString();
                    }

                    if (twin?.Tags?.Contains("email") ?? false)
                    {
                        deviceAttr.AppartmentOwnerEmail = twin?.Tags?["email"].ToString();

                    }
                    deviceAttr.Etag = twin?.ETag;
                    deviceAttr.DeviceId = deviceId;
                    return deviceAttr;
                }
                catch (Exception ex)
                {
                    log.LogError($"GetDeviceTwin-{deviceId}-{ex.Message}", ex, $"Error-{typeof(IoTDeviceProvider)}");
                    retryCount++;
                    if (retryCount > 5)
                        return null;
                    await Task.Delay(randomDelay.Next(1000, 2000));
                }
            }
        }

        public async Task<List<IoTDevice>> GetDevices()
        {
            var query = $"SELECT  deviceId,  lastActivityTime,tags.email,tags.apptno FROM devices";
            var devicequery = Manager.CreateQuery(query);
            var devices = await devicequery.GetNextAsJsonAsync();
            var result = devices?.Select(x =>
            {
                var device = JsonConvert.DeserializeObject<IoTDevice>(x);
                return device ?? null;
            }).ToList();

            return result;
        }
        public async Task<List<IoTDevice>> GetDeviceByConnectivityStatus(bool showConnected)
        {
            var query = showConnected ? $"SELECT deviceId,  lastActivityTime,tags.email,tags.apptno FROM devices WHERE connectionState ='Connected'" :
                $"SELECT deviceId,  lastActivityTime,tags.email,tags.apptno FROM devices WHERE connectionState ='Disconnected'";
            var devicequery = Manager.CreateQuery(query);
            var devices = await devicequery.GetNextAsJsonAsync();
            var deviceData = devices.ToList();

            var result = deviceData?.Select(x =>
            {
                var device = JsonConvert.DeserializeObject<IoTDevice>(x);
                return device ?? null;
            }).ToList();

            return result;
        }

    }
}
