using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace SWMSB.COMMON
{
    public class Alert
    {
        [Required]
        [JsonProperty("waterusage")]
        public double Waterusage { get; set; }
        [Required]

        [JsonProperty("dev_id")]
        public string DevId { get; set; }
        [Required]

        [JsonProperty("alerttype")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AlertType AlertType { get; set; }

        public string ToIntendedJsonString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    public class EmailMsg
    {
        [Required]
        public string ReceiverEmail { get; set; }
        [Required]

        public string Subject { get; set; }
        [Required]

        public string EmailBody { get; set; }
        [Required]

        public string ReceiverName { get; set; }
    }
    public enum AlertType
    {
        ALERT_FIFTY_G,
        ALERT_THOUSAND_G
    }

    public class DeviceAttribute
    {
        public string DeviceId { get; set; }
        public string Appartment { get; set; }

        public string AppartmentOwnerEmail { get; set; }
        public string Etag { get; set; }
        public string ToIntendedJsonString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

    }
}
