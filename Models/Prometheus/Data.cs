using Newtonsoft.Json;

namespace HPI.BBB.Autoscaler.Models.Prometheus
{
   public class Data
    {
        [JsonProperty("resultType")]
        public string ResultType { get; set; }

        [JsonProperty("result")]
        public Result[] Result { get; set; }
    }

}
