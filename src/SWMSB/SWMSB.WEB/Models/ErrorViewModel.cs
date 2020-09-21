using SWMSB.PROVIDERS;
using System;
using System.Collections.Generic;

namespace SWMSB.WEB.Models
{
    public class MonthlyData
    {
        public List<TelemetryStorage> Data { get; set; }
        public MonthlyData()
        {
            Data = new List<TelemetryStorage>();
        }

        public string MonthYr { get; set; }
    }
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}