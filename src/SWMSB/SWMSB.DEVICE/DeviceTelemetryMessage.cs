using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWMSB.DEVICE
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
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
    public enum AlertType
    {
        ALERT_FIFTY_G,
        ALERT_THOUSAND_G
    }

    public class DeviceTelemetryMsg
    {


        [JsonProperty("deviceid")]
        public string Deviceid { get; set; }

        [JsonProperty("tenantid")]
        public string Tenantid { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("waterusage")]
        public string Waterusage { get; set; }

        [JsonProperty("messagegenerated")]
        public long MessageGenerated { get; set; }

        public string ToIntendedJsonString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

    }

    public class TelemetryInterval
    {
        [JsonProperty("waterusage")]
        public string Waterusage { get; set; }


        [JsonProperty("timestamp")]
        public string MessageGenerated { get; set; }
    }

    public class DeviceTelemetryDocument : Resource
    {
        public DeviceTelemetryDocument()
        {
            Intervals = new List<TelemetryInterval>();
            DocumentType = nameof(DeviceTelemetryDocument);
        }

        [JsonProperty("intervals")]
        public List<TelemetryInterval> Intervals { get; set; }

        [JsonProperty("deviceid")]
        public string Deviceid { get; set; }

        [JsonProperty("tenantid")]
        public string Tenantid { get; set; }

        [JsonProperty("documenttype")]
        public string DocumentType { get; protected set; }

        [JsonProperty("date")]
        public string Date { get; set; }
        public string ToIntendedJsonString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static string GetDocumentKey(string id)
        {
            return $"{nameof(DeviceTelemetryDocument)}:{id}";
        }
    }


}
