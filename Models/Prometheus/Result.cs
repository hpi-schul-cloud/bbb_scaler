using Newtonsoft.Json;
using System.Collections.Generic;

namespace HPI.BBB.Autoscaler.Models.Prometheus
{
    public class Result
    {
        [JsonProperty("metric")]
        public Metric Metric { get; set; }

        [JsonProperty("value")]
        public List<float> Value { get; set; }
    }

}
