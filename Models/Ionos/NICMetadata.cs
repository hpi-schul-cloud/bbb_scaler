using Newtonsoft.Json;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    class NICMetadata
    {
        [JsonProperty("createdDate")]
        public string CreatedDate { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("createdByUserId")]
        public string CreatedByUserId { get; set; }

        [JsonProperty("etag")]
        public string Etag { get; set; }

        [JsonProperty("lastModifiedDate")]
        public string LastModifiedDate { get; set; }

        [JsonProperty("lastModifiedBy")]
        public string LastModifiedBy { get; set; }

        [JsonProperty("lastModifiedByUserId")]
        public string LastModifiedByUserId { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }
}