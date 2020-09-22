using Newtonsoft.Json;
using SWMSB.PROVIDERS;
using System;
using System.Collections.Generic;

namespace SWMSB.WEB.Models
{
    public class DailyWaterUsage
    {
        public string DailySumWaterUsage { get; set; }
        public string DailyAvgWaterUsage { get; set; }
        public string Days { get; set; }

        public DailyWaterUsage()
        {
         
        }

        public string DeviceId { get; set; }
        public string MonthYr { get; set; }
        public string ToIntendedJsonString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    public class MonthlyData
    {
        public List<TelemetryStorage> Data { get; set; }
        public MonthlyData()
        {
            Data = new List<TelemetryStorage>();
        }

        public string DeviceId { get; set; }
        public string MonthYr { get; set; }
        public double TotalWaterUsage { get; set; }
        public double AvgWaterUsage { get; set; }
        public string ToIntendedJsonString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}