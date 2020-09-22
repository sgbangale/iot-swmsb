using Newtonsoft.Json;
using SWMSB.COMMON;
using System.ComponentModel.DataAnnotations;

namespace SWMSB.DEVICE
{
    public class DeviceRequest : BackendRequest
    {
        [Required]
        public string DeviceId { get; set; }
        public TTNUpLinkPayload TTNPayload { get; set; }
    }
    public class PayloadFields
    {
        [Required]
        [JsonProperty("waterusage")]
        [Range(0.1, 1000)]
        public double Waterusage { get; set; }
        [JsonProperty("reset")]
        public bool Reset{ get; set; }
    }

    public class Metadata
    {
        [JsonProperty("time")]
        public string Time { get; set; }
    }

    public class TTNDownLinkPayload
    {
        [Required]
        [JsonProperty("confirmed")]
        public bool Confirmed { get; set; }
        [Required]

        [JsonProperty("port")]
        public int Port { get; set; }
        [Required]
        [JsonProperty("dev_id")]
        public string DevId { get; set; }
        [Required]
        [JsonProperty("payload_fields")]
        public PayloadFields PayloadFields { get; set; }
        public string ToIntendedJsonString()
        {
            if (this == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public string ToJsonString()
        {
            if (this == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }

    public class TTNUpLinkPayload
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
}