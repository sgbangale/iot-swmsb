using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;

namespace SWMSB.DEVICE
{

    public partial class DeviceTwinMsg
    {
        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("tags")]
        public Tags Tags { get; set; }
        public string ToIntendedJsonString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public partial class Tags
    {
        [JsonProperty("apptno")]
        public string Apptno { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
