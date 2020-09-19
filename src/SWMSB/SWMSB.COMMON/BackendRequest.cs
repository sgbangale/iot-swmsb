using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SWMSB.COMMON
{
    public enum BackendRequestType
    {
        REGISTER_DEVICE_TO_IOTHUB,
        DELETE_DEVICE_IOTHUB,
        ENABLE_DEVICE,
        DISABLE_DEVICE
    }

    public class BackendRequest
    {
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public BackendRequestType Type { get; set; }
      
        public string ToIntendedJsonString()
        {
            if (this == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    
}
