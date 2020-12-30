using HPI.BBB.Autoscaler.Models;
using HPI.BBB.Autoscaler.Models.Ionos;
using System;
using System.Collections.Generic;

namespace HPI.BBB.Autoscaler.Models
{
    internal class MachineDataCenterTuple
    {
        public string DataCenter { get; }
        public IonosMachine Machine { get; }

        public MachineDataCenterTuple(string dc, IonosMachine m)
        {
            DataCenter = dc;
            Machine = m;
        }

        public override bool Equals(object obj)
        {
            return obj is WorkloadMachineTuple other &&
                   EqualityComparer<IonosMachine>.Default.Equals(Machine, other.Machine) &&
                   EqualityComparer<string>.Default.Equals(DataCenter, other.DataCenter);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DataCenter, Machine);
        }
    }
}
