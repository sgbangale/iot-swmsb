using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SWMSB.COMMON;
using SWMSB.DEVICE;
using SWMSB.PROVIDERS;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace SWMSB.BAL
{
    public interface IIotHubManagerRepository
    {
        Task<bool> IsDeviceRegisteredAsync(string deviceId);
        Task<IoTHubDeviceResultStatus> AddDeviceAsync(string deviceId);
        Task<IoTHubDeviceResultStatus> SendEventAsync(TTNUpLinkPayload ttnPayload);
        Task<DeviceAttribute> UpdateDeviceTwinAsync(DeviceAttribute deviceAttribute);
        Task<DeviceAttribute> GetDeviceTwinAsync(DeviceAttribute deviceAttribute);

        Task<List<IoTDevice>> GetDevicesForPieChart(int cacheRefreshRateInMinutes);
        Task<int> GetTotalDevicesForPieChart(int cacheRefreshRateInMinutes);
        Task<List<IoTDevice>> GetAllDevices(int cacheRefreshRateInMinutes);


    }

    public sealed class IotHubManagerRepository : IIotHubManagerRepository
    {
        ILogger logger;
        private IoTHubProvider iotHubManager;

        public Config Config { get; set; }

        public IotHubManagerRepository(Config _config, ILogger _logger)
        {
            Config = _config;
            logger = _logger;
            iotHubManager = new IoTHubProvider(Config, _logger);
        }


        public async Task<bool> IsDeviceRegisteredAsync(string deviceId)
        {
            return await iotHubManager.IsDeviceRegisteredAsync(deviceId);
        }

        public async Task<IoTHubDeviceResultStatus> AddDeviceAsync(string deviceId)
        {
            return await iotHubManager.AddDeviceAsync(deviceId);
        }

        public async Task<IoTHubDeviceResultStatus> SendEventAsync(TTNUpLinkPayload ttnPayload)
        {
            //validation of payload
            Validation ttnPayloadValidationResult = ttnPayload.Validate();
            Validation payloadvaliation = ttnPayload.PayloadFields.Validate();
            if (!ttnPayloadValidationResult.Success)
            {
                logger.LogInformation(ttnPayloadValidationResult.ErrorMessage);
                return IoTHubDeviceResultStatus.INVALID_REQUEST;
            }
            else if (!payloadvaliation.Success)
            {
                logger.LogInformation(payloadvaliation.ErrorMessage);
                return IoTHubDeviceResultStatus.INVALID_REQUEST;
            }

            IoTDeviceProvider ioTHubDeviceClientProvider = new IoTDeviceProvider(Config, ttnPayload.DevId, logger);

            return await ioTHubDeviceClientProvider.SendEventAsync(ttnPayload);

        }


        public async Task<DeviceAttribute> UpdateDeviceTwinAsync(DeviceAttribute deviceAttribute)
        {
            if (!string.IsNullOrEmpty(deviceAttribute.DeviceId))
                return await HandleCache(deviceAttribute);

            return null;
        }

        private async Task<DeviceAttribute> HandleCache(DeviceAttribute deviceAttribute)
        {
            var devid = deviceAttribute.DeviceId.Trim();
            var cache = MemoryCache.Default.GetCacheItem(devid);
            if (cache == null)
            {
                var result = await iotHubManager.UpdateDeviceTwinAsync(deviceAttribute);

                MemoryCache.Default.Add(new CacheItem(devid, result.ToIntendedJsonString()), new CacheItemPolicy()
                {
                    SlidingExpiration = new TimeSpan(hours: 1, 0, 0)
                });

                return result;
            }
            else
            {
                return JsonConvert.DeserializeObject<DeviceAttribute>(cache.Value.ToString());
            }
        }

        public async Task<DeviceAttribute> GetDeviceTwinAsync(DeviceAttribute deviceAttribute)
        {
            if (!string.IsNullOrEmpty(deviceAttribute.DeviceId))
            {
                return await HandleGetTwinCache(deviceAttribute);
            }

            return null;
        }

        private async Task<DeviceAttribute> HandleGetTwinCache(DeviceAttribute deviceAttribute)
        {
            var devid = deviceAttribute.DeviceId.Trim();
            var cache = MemoryCache.Default.GetCacheItem(devid);

            if (cache == null)
            {
                var result = await iotHubManager.GetDeviceTwin(deviceAttribute.DeviceId);
                MemoryCache.Default.Add(new CacheItem(devid, result.ToIntendedJsonString()), new CacheItemPolicy()
                {
                    SlidingExpiration = new TimeSpan(hours: 1, 0, 0)
                });
                return result;
            }
            else
            {
                return JsonConvert.DeserializeObject<DeviceAttribute>(cache.Value.ToString());
            }
        }

        public async Task<List<IoTDevice>> GetDevicesForPieChart(int cacheRefreshRateInMinutes)
        {
            if (!MemoryCache.Default.Contains("PIECHART_ACTIVE_METERS"))
            {
               var activeDevices = await iotHubManager.GetDeviceByConnectivityStatus(true);

                MemoryCache.Default.Set(new CacheItem("PIECHART_ACTIVE_METERS", activeDevices), new CacheItemPolicy()
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(cacheRefreshRateInMinutes)
                });

                return activeDevices;
            }
            else
            {
               return MemoryCache.Default.Get("PIECHART_ACTIVE_METERS") as List<IoTDevice>;

            }
        }

        public async Task<List<IoTDevice>> GetAllDevices(int cacheRefreshRateInMinutes)
        {
            if (!MemoryCache.Default.Contains("ALL_METERS"))
            {
                var activeDevices = await iotHubManager.GetDevices();

                MemoryCache.Default.Set(new CacheItem("ALL_METERS", activeDevices), new CacheItemPolicy()
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(cacheRefreshRateInMinutes)
                });

                return activeDevices;
            }
            else
            {
                return MemoryCache.Default.Get("ALL_METERS") as List<IoTDevice>;

            }
        }

        public async Task<int> GetTotalDevicesForPieChart(int cacheRefreshRateInMinutes)
        {
            if (!MemoryCache.Default.Contains("TOTAL_DEVICES_COUNT"))
            {

                var devices = await iotHubManager.GetDevices();

                MemoryCache.Default.Set(new CacheItem("TOTAL_DEVICES_COUNT", devices.Count), new CacheItemPolicy()
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(cacheRefreshRateInMinutes)
                });

                return devices.Count;
            }
            else
            {
                return int.Parse(MemoryCache.Default.Get("TOTAL_DEVICES_COUNT")?.ToString());
            }

        }
    }
}
