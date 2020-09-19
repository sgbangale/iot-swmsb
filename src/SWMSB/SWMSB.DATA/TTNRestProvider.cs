using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace SWMSB.DATA
{
    public class TTNRestProvider
    {
        public static T QueryTTNServer<T>(string ttnEndpoint, string ttnApiKey, string interval = "2d")
        {
            var uri = $"{ttnEndpoint}/api/v2/query?last={interval}";
            var client = new RestClient(uri);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", $"key {ttnApiKey}");

            request.AddHeader("Content-Type", "application/json");
            IRestResponse response = client.Execute(request);
            return
                response.StatusCode == System.Net.HttpStatusCode.OK ?
             JsonConvert.DeserializeObject<T>(response.Content) :  default;
        }
    }
}
