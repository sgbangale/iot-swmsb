using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SWMSB.BAL;
using SWMSB.COMMON;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SWMSB.PROCESSORS
{
    public static class MessageProcessor
    {
        static MessageProcessor()
        {
            Config = new Config();
        }

        public static Config Config { get; }

        [FunctionName(ProcessorsConstants.MessageProcessor)]
        public static async Task Run(
            [EventHubTrigger(ProcessorsConstants.MsgEventHubName,
            ConsumerGroup = ProcessorsConstants.ConsumerGroupName,
            Connection = ProcessorsConstants.EventHubEndPoint)] EventData[] events,
            ILogger log)
        {
            var exceptions = new List<Exception>();
            BackendRepository backendRepository = new BackendRepository(Config, log);

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                    var _alerts = JsonConvert.DeserializeObject<Alert[]>(messageBody);
                    foreach (var alert in _alerts)
                    {
                        var alertValidationResult = alert.Validate();
                        if (alertValidationResult.Success)
                        {
                            switch (alert.AlertType)
                            {
                                case AlertType.ALERT_FIFTY_G:
                                    await backendRepository.SendLeakEmailAsync(alert);
                                    break;
                                case AlertType.ALERT_THOUSAND_G:
                                    await backendRepository.SendUsageExhaustEmailAsync(alert);
                                    break;
                                default:
                                    break;
                            }
                            log.LogInformation($"{typeof(MessageProcessor)} processed a message: {alert.ToIntendedJsonString()}");

                        }
                        else
                        {
                            log.LogError($"{typeof(MessageProcessor)} model validation failed-{alertValidationResult.ErrorMessage}: {alert.ToIntendedJsonString()}");
                        }
                    }
                    await Task.Yield();
                }
                catch (Exception e)
                { 
                    exceptions.Add(e);
                    log.LogError($"{typeof(MessageProcessor)} exception:", e.Message);
                }
            }
        }

    }
}
