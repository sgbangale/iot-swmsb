using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using SWMSB.COMMON;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWMSB.DEVICE
{
    public class DeviceRequest : BackendRequest
    {
        [Required]
        public string DeviceId { get; set; }
        public Root TTNPayload { get; set; }
    }
    public class PayloadFields
    {
        [Required]
        [JsonProperty("waterusage")]
        [Range(0.1,1000)]
        public double Waterusage { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("time")]
        public string Time { get; set; }
    }

    public class Root
    {
        public string ToIntendedJsonString()
        {
            if (this == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        [JsonProperty("app_id")]
        public string AppId { get; set; }

        [Required]
        [JsonProperty("dev_id")]
        public string DevId { get; set; }

        [JsonProperty("hardware_serial")]
        public string HardwareSerial { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("counter")]
        public int Counter { get; set; }

        [JsonProperty("payload_raw")]
        public string PayloadRaw { get; set; }
        [Required]
        [JsonProperty("payload_fields")]
        public PayloadFields PayloadFields { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("downlink_url")]
        public string DownlinkUrl { get; set; }
    }

    public class TTNDeviceTelemetry
    {
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }

        [JsonProperty("raw")]
        public string Raw { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }
    }

    public class RootTTNDeviceTelemetries
    {
        [JsonProperty("TTNDeviceTelemetries")]
        public List<TTNDeviceTelemetry> TTNDeviceTelemetries { get; set; }
    }



}