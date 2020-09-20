using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
            var config = new Config();


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
                                    await SendFiftyGallonEmail(alert,Config);
                                    break;
                                case AlertType.ALERT_THOUSAND_G:
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
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                    log.LogError($"{typeof(MessageProcessor)} exception:", e.Message);
                }
            }
        }

        private static async Task SendFiftyGallonEmail(Alert alert, Config config)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.sendgrid.com/v3/mail/send"))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {config.SENDGRID_API_KEY}");

                    request.Content = new StringContent("{\"personalizations\": [{\"to\": [{\"email\": \"SendFiftyGallon@yopmail.com\"}]}],\"from\": {\"email\": \"swmsb@swmsb.com\"},\"subject\": \"Leak Detetction\",\"content\": [{\"type\": \"text/plain\", \"value\": +and easy to do anywhere, even with cURL\"}]}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var response = await httpClient.SendAsync(request);
                }
            }
        }
    }
}
