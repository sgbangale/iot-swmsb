using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SWMSB.BAL;
using SWMSB.COMMON;

namespace SWMSB.PROCESSORS
{
    public static class DataCollector
    {
        [FunctionName(ProcessorsConstants.DataCollector)]
        public static void Run([TimerTrigger("0 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            var config = new Config();
            IDeviceTelemetryRepository telemetryRepository = new DeviceTelemetryRepository(config, log);
            telemetryRepository.StoreTTNTelemetries();
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
