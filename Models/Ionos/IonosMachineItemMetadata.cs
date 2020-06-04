using Newtonsoft.Json;
using System;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    public class IonosMachineItemMetadata
    {
        [JsonProperty("etag")]
        public string Etag { get; set; }

        [JsonProperty("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("createdByUserId")]
        public Guid CreatedByUserId { get; set; }

        [JsonProperty("lastModifiedDate")]
        public DateTimeOffset LastModifiedDate { get; set; }

        [JsonProperty("lastModifiedBy")]
        public string LastModifiedBy { get; set; }

        [JsonProperty("lastModifiedByUserId")]
        public Guid LastModifiedByUserId { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }
}