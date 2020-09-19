using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;

namespace SWMSB.DEVICE
{
    public class DeviceTwinMsg
    {
        [JsonProperty("version")]
        public int Version { get; set; }
        [JsonProperty("properties")]
        public Properties Properties { get; set; }
        public string ToIntendedJsonString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    public class DeviceTwinDocument: Resource
    {
        [JsonProperty("version")]
        public int Version { get; set; }
        [JsonProperty("properties")]
        public Properties Properties { get; set; }
        public string ToIntendedJsonString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public DeviceTwinDocument(string id)
        {
            Id = GetDocumentKey(id);
        }
        public static string GetDocumentKey(string id)
        {
            return $"{nameof(DeviceTwinDocument)}:{id}";
        }
    }

    public class Properties
    {
        [JsonProperty("desired")]
        public Desired Desired { get; set; }
        [JsonProperty("reported")]
        public Reported Reported { get; set; }
    }

    public class Desired
    {
        [JsonProperty("latestotaversion")]
        public string LatestOTAVersion { get; set; }
        [JsonProperty("pauseusage")]
        public bool PauseUsage { get; set; }
    }



    public class Reported
    {
        [JsonProperty("tenantid")]

        public string Tenantid { get; set; }
        [JsonProperty("ota")]

        public Ota Ota { get; set; }

    }

    public class Ota
    {
        [JsonProperty("otaversion")]
        public string OtaVersion { get; set; }
        [JsonProperty("progress")]
        public string Progress { get; set; }
    }
}
