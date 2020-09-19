using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SWMSB.BAL;
using SWMSB.COMMON;
using SWMSB.DEVICE;

namespace SWMSB.PROCESSORS
{
    class TwinProcessor
    {

        [FunctionName(ProcessorsConstants.TwinProcessor)]
        public static async Task Run([EventHubTrigger(ProcessorsConstants.TwinEventHubName, ConsumerGroup = ProcessorsConstants.ConsumerGroupName, Connection = ProcessorsConstants.EventHubEndPoint)] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();
            var dbSecrete = new DocumentDbSecret();
            IDeviceTwinRepository twinRepository = new DeviceTwinRepository(dbSecrete, log);
            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                    var msg = JsonConvert.DeserializeObject<DeviceTwinMsg>(messageBody);

                    twinRepository.TwinMsgReceived(msg);


                    log.LogInformation($"{typeof(TwinProcessor)} processed a message: {messageBody}");
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                    log.LogError($"{typeof(TwinProcessor)} exception:", e.Message);
                }
            }
        }

    }
}
