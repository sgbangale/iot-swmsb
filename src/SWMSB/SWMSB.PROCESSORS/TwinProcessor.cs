using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SWMSB.BAL;
using SWMSB.COMMON;
using SWMSB.DEVICE;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace SWMSB.PROCESSORS
{
    class TwinProcessor
    {
        static TwinProcessor()
        {
            Config = new Config();
        }

        public static Config Config { get; }

        [FunctionName(ProcessorsConstants.TwinProcessor)]
        public static async Task Run([EventHubTrigger(ProcessorsConstants.TwinEventHubName, ConsumerGroup = ProcessorsConstants.ConsumerGroupName, Connection = ProcessorsConstants.EventHubEndPoint)] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();
         
            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    DeviceTwinMsg twin= JsonConvert.DeserializeObject<DeviceTwinMsg>(messageBody);
                    if (twin != null)
                    {
                        if (eventData.Properties.ContainsKey("deviceId"))
                        {
                            IotHubManagerRepository iotHubManagerRepository = new IotHubManagerRepository(Config, log);
                            var updateCache = new DeviceAttribute
                            {
                                Appartment = twin.Tags.Apptno,
                                AppartmentOwnerEmail = twin.Tags.Email,
                                DeviceId = eventData.Properties["deviceId"].ToString(),
                                Etag = null

                            };
                            MemoryCache.Default.Set(new CacheItem(updateCache.DeviceId, updateCache.ToIntendedJsonString()), new CacheItemPolicy()
                            {
                                SlidingExpiration = new TimeSpan(hours: 1, 0, 0)
                            });
                        }
                        
                    }

                    log.LogInformation($"{typeof(TwinProcessor)} processed a message: {messageBody}");
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    log.LogError($"{typeof(TwinProcessor)} exception:", e.Message);
                }
            }
        }

    }
}
