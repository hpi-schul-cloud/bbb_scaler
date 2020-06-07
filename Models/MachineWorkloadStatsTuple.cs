using HPI.BBB.Autoscaler.Models.BBB;
using HPI.BBB.Autoscaler.Models.Ionos;
using System;
using System.Collections.Generic;

namespace HPI.BBB.Autoscaler.Models
{
    internal class MachineWorkloadStatsTuple
    {
        public IonosMachine Machine { get; }
        public Workload Workload { get; }
        public List<Meeting> Stats { get; }

        public MachineWorkloadStatsTuple(IonosMachine machine, Workload workload, List<Meeting> stats)
        {
            Machine = machine;
            Workload = workload;
            Stats = stats;
        }

        public override bool Equals(object obj)
        {
            return obj is MachineWorkloadStatsTuple other &&
                   EqualityComparer<IonosMachine>.Default.Equals(Machine, other.Machine) &&
                   EqualityComparer<Workload>.Default.Equals(Workload, other.Workload) &&
                   EqualityComparer<List<Meeting>>.Default.Equals(Stats, other.Stats);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Machine, Workload, Stats);
        }
    }
}