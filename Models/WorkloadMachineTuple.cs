using HPI.BBB.Autoscaler.Models;
using HPI.BBB.Autoscaler.Models.Ionos;
using System;
using System.Collections.Generic;

namespace HPI.BBB.Autoscaler.Models
{
    internal class WorkloadMachineTuple
    {
        public IonosMachine Machine { get; }
        public Workload Workload { get; }

        public WorkloadMachineTuple(IonosMachine m, Workload w)
        {
            Machine = m;
            Workload = w;
        }

        public override bool Equals(object obj)
        {
            return obj is WorkloadMachineTuple other &&
                   EqualityComparer<IonosMachine>.Default.Equals(Machine, other.Machine) &&
                   EqualityComparer<Workload>.Default.Equals(Workload, other.Workload);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Machine, Workload);
        }
    }
}
