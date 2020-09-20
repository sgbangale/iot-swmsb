using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SWMSB.BAL;
using SWMSB.COMMON;
using SWMSB.DEVICE;
using System;
using System.IO;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace SWMSB.PROCESSORS
{
    public static class IoTBridge
    {
        private static readonly Config config;
        private static MemoryCache LocalCache = MemoryCache.Default;
        static IoTBridge()
        {
            config = new Config();
        }
        [FunctionName("IoTBridge")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            ILogger log)
        {
            try
            {
                //create config instance and initilize repository
                TTNRepository ttnRepository = new TTNRepository(config, log, LocalCache);

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                TTNUpLinkPayload ttnPayload = JsonConvert.DeserializeObject<TTNUpLinkPayload>(requestBody);

                if (ttnPayload != null)
                {
                    log.LogTrace(ttnPayload.ToIntendedJsonString());
                  
                    //send msg to repository method
                    var result = await ttnRepository.TelemetryMsgReceivedAsync(ttnPayload);
                    return new OkObjectResult(result.ToString());
                }
                else
                {
                    log.LogTrace($"invalid request-{requestBody}");
                    return new BadRequestObjectResult(IoTHubDeviceResultStatus.INVALID_REQUEST);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Error-{typeof(IoTBridge)}");
                var result = new ObjectResult("Internal Server Error");
                result.StatusCode = StatusCodes.Status500InternalServerError;
                return result;
            }
        }
    }
}