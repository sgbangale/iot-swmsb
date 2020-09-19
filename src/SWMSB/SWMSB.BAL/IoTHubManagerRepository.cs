using AutoMapper;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SWMSB.COMMON;
using SWMSB.DATA;
using SWMSB.DEVICE;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace SWMSB.BAL
{
    public interface IiotHubManagerRepository
    {

        Task<bool> IsDeviceRegisteredAsync(string deviceId);
        Task<IoTHubDeviceResultStatus> AddDeviceAsync(string deviceId);

        Task<IoTHubDeviceResultStatus> SendEventAsync(Root ttnPayload);
    }

    public sealed class IotHubManagerRepository : IiotHubManagerRepository
    {
        ILogger logger;
        private IoTHubManager iotHubManager;

        public Config Config { get; set; }

        public IotHubManagerRepository(Config _config, ILogger _logger)
        {
            Config = _config;
            logger = _logger;
            iotHubManager = new IoTHubManager(Config.IOT_HUB_CS, _logger);
        }

        
        public static void AddOrUpdateToCache(string deviceid)
        {
            if (!string.IsNullOrEmpty(deviceid))
            {
                var devid = deviceid.Trim();
                if (!MemoryCache.Default.Contains(deviceid))
                {
                    MemoryCache.Default.Add(new CacheItem(devid, string.Empty), new CacheItemPolicy()
                    {
                        SlidingExpiration = new TimeSpan(hours: 1, 0, 0)
                    });
                }
                else
                {
                    MemoryCache.Default.Set(new CacheItem(devid, string.Empty), new CacheItemPolicy()
                    {
                        SlidingExpiration = new TimeSpan(hours: 1, 0, 0)
                    });
                }
            }
       
        }

        public async Task<bool> IsDeviceRegisteredAsync(string deviceId)
        {
            return await iotHubManager.IsDeviceRegisteredAsync(deviceId);
        }

        public async Task<IoTHubDeviceResultStatus> AddDeviceAsync(string deviceId)
        {
            return await iotHubManager.AddDeviceAsync(deviceId);
        }

        public async Task<IoTHubDeviceResultStatus> SendEventAsync(Root ttnPayload)
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

            IoTHubDeviceClientProvider ioTHubDeviceClientProvider = new IoTHubDeviceClientProvider(Config,ttnPayload.DevId, logger);

            return await ioTHubDeviceClientProvider.SendEventAsync(ttnPayload);

        }
    }
}
