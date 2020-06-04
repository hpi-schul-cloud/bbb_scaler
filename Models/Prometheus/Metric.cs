using Newtonsoft.Json;

namespace HPI.BBB.Autoscaler.Models.Prometheus
{
    public class Metric
    {
        [JsonProperty("instance")]
        public string Instance { get; set; }
    }

}
