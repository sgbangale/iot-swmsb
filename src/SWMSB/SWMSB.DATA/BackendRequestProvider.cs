using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SWMSB.COMMON;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SWMSB.DATA
{
    public class BackendRequestProvider
    {
        public BackendRequestProvider(string backendRequestConnectionString, ILogger _logger)
        {
            log = _logger;
            RequestProvider = EventHubClient.CreateFromConnectionString(backendRequestConnectionString);
        }

        private readonly ILogger log;

        public EventHubClient RequestProvider { get; }

        public async Task<bool> TriggerRequestAsync(BackendRequest backendRequest)
        {
            var validationResult = backendRequest.Validate();
            if (validationResult.Success)
            {
                try
                {
                    var eventMessage = JsonConvert.SerializeObject(backendRequest);
                    await RequestProvider.SendAsync(new EventData(Encoding.UTF8.GetBytes(eventMessage)));

                    return true;
                }
                catch (Exception ex)
                {
                    log.LogError($"{backendRequest.Type}-{ex.Message}", ex, $"Error-{typeof(BackendRequestProvider)}");
                    return false;
                }
            }
            else
            {
                log.LogError($"{typeof(BackendRequestProvider)}-validation-failed-{validationResult.ErrorMessage}");
                return false;
            }
          
        }


    }
}
