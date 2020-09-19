using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SWMSB.BAL;
using SWMSB.COMMON;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SWMSB.PROCESSORS
{
    public static class BackendRequestProcessor
    {

        [FunctionName(ProcessorsConstants.BackendRequestProcessor)]
        public static async Task Run(
            [EventHubTrigger("device-swmsb-requests",
            ConsumerGroup = ProcessorsConstants.ConsumerGroupName,
            Connection = ProcessorsConstants.EventHubEndPoint)] EventData[] events,
            ILogger log)
        {
            var exceptions = new List<Exception>();
            var config = new Config();
            IBackendRepository backendRepository = new BackendRepository(config, log);

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);


                    var result = await backendRepository.BackendMsgReceivedAsync(messageBody);
                    log.LogInformation($"{typeof(BackendRequestProcessor)} backend request response- {result.ToString()}");
                    await Task.Yield();
                }
                catch (Exception e)
                {   
                    exceptions.Add(e);
                    log.LogError($"{typeof(BackendRequestProcessor)} exception:", e.Message);
                }
            }
        }
    }
}
