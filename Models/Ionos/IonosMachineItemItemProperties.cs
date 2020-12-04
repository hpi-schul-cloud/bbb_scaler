using Newtonsoft.Json;
using System;

namespace HPI.BBB.Autoscaler.Models.Ionos
{
    public class IonosMachineItemItemProperties
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mac", NullValueHandling = NullValueHandling.Ignore)]
        public string Mac { get; set; }

        [JsonProperty("ips", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Ips { get; set; }

        [JsonProperty("dhcp", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Dhcp { get; set; }

        [JsonProperty("lan", NullValueHandling = NullValueHandling.Ignore)]
        public long? Lan { get; set; }

        [JsonProperty("firewallActive", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FirewallActive { get; set; }

        [JsonProperty("nat", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Nat { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("availabilityZone", NullValueHandling = NullValueHandling.Ignore)]
        public string AvailabilityZone { get; set; }

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public string Image { get; set; }

        [JsonProperty("imagePassword")]
        public object ImagePassword { get; set; }

        [JsonProperty("sshKeys")]
        public object SshKeys { get; set; }

        [JsonProperty("bus", NullValueHandling = NullValueHandling.Ignore)]
        public string Bus { get; set; }

        [JsonProperty("licenceType", NullValueHandling = NullValueHandling.Ignore)]
        public string LicenceType { get; set; }

        [JsonProperty("cpuHotPlug", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CpuHotPlug { get; set; }

        [JsonProperty("ramHotPlug", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RamHotPlug { get; set; }

        [JsonProperty("nicHotPlug", NullValueHandling = NullValueHandling.Ignore)]
        public bool? NicHotPlug { get; set; }

        [JsonProperty("nicHotUnplug", NullValueHandling = NullValueHandling.Ignore)]
        public bool? NicHotUnplug { get; set; }

        [JsonProperty("discVirtioHotPlug", NullValueHandling = NullValueHandling.Ignore)]
        public bool? DiscVirtioHotPlug { get; set; }

        [JsonProperty("discVirtioHotUnplug", NullValueHandling = NullValueHandling.Ignore)]
        public bool? DiscVirtioHotUnplug { get; set; }

        [JsonProperty("deviceNumber", NullValueHandling = NullValueHandling.Ignore)]
        public long? DeviceNumber { get; set; }
    }
}
