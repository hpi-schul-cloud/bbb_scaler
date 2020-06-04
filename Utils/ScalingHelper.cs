using HPI.BBB.Autoscaler.APIs;
using HPI.BBB.Autoscaler.Models;
using HPI.BBB.Autoscaler.Models.Ionos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HPI.BBB.Autoscaler.Utils
{
    public class ScalingHelper
    {

        private static readonly int MINIMUM_ACTIVE_MACHINES = int.Parse(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MINIMUM_ACTIVE_MACHINES")) ? "2"
            : Environment.GetEnvironmentVariable("MINIMUM_ACTIVE_MACHINES"), CultureInfo.InvariantCulture);
        private static readonly float MAX_ALLOWED_MEMORY_WORKLOAD = float.Parse(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MAX_ALLOWED_MEMORY_WORKLOAD")) ? "0.35"
            : Environment.GetEnvironmentVariable("MAX_ALLOWED_MEMORY_WORKLOAD"), CultureInfo.InvariantCulture);
        private static readonly float MAX_ALLOWED_CPU_WORKLOAD = float.Parse(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MAX_ALLOWED_CPU_WORKLOAD")) ? "0.35"
            : Environment.GetEnvironmentVariable("MAX_ALLOWED_CPU_WORKLOAD"), CultureInfo.InvariantCulture);
        private static readonly float MIN_ALLOWED_MEMORY_WORKLOAD = float.Parse(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MIN_ALLOWED_MEMORY_WORKLOAD")) ? "0.15"
            : Environment.GetEnvironmentVariable("MIN_ALLOWED_MEMORY_WORKLOAD"), CultureInfo.InvariantCulture);
        private static readonly float MIN_ALLOWED_CPU_WORKLOAD = float.Parse(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MIN_ALLOWED_CPU_WORKLOAD")) ? "0.05"
            : Environment.GetEnvironmentVariable("MIN_ALLOWED_CPU_WORKLOAD"), CultureInfo.InvariantCulture);
        private static readonly int MAX_WORKER_MEMORY = int.Parse(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MAX_WORKER_MEMORY")) ? "16384"
            : Environment.GetEnvironmentVariable("MAX_WORKER_MEMORY"), CultureInfo.InvariantCulture);
        private static readonly int DEFAULT_WORKER_MEMORY = int.Parse(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEFAULT_WORKER_MEMORY")) ? "8192"
                    : Environment.GetEnvironmentVariable("DEFAULT_WORKER_MEMORY"), CultureInfo.InvariantCulture);
        private static readonly int MAX_WORKER_CPU = int.Parse(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MAX_WORKER_CPU")) ? "4"
            : Environment.GetEnvironmentVariable("MAX_WORKER_CPU"), CultureInfo.InvariantCulture);
        private static readonly int DEFAULT_WORKER_CPU = int.Parse(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEFAULT_WORKER_CPU")) ? "2"
                    : Environment.GetEnvironmentVariable("DEFAULT_WORKER_CPU"), CultureInfo.InvariantCulture);

        internal static void ShutDown(ILogger log, List<WorkloadMachineTuple> totalWorkload, BBBAPI bbb, IonosAPI ionos, string ionosDataCenter)
        {
            var shutDown = totalWorkload
                        .OrderByDescending(m => new { m.Workload.MemoryUtilization, m.Workload.CPUUtilization })
                        .Skip(MINIMUM_ACTIVE_MACHINES)
                        .Select(async m => new MachineWorkloadStatsTuple(m.Machine, m.Workload, await bbb.GetMeetingsAsync(m.Machine.PrimaryIP).ConfigureAwait(false)))
                        .Select(res => res.Result)
                        .Where(m => m.Workload.MemoryUtilization < MIN_ALLOWED_MEMORY_WORKLOAD &&
                        m.Workload.CPUUtilization < MIN_ALLOWED_CPU_WORKLOAD && m.Machine.Properties.Cores <= DEFAULT_WORKER_CPU &&
                        m.Machine.Properties.Ram <= DEFAULT_WORKER_MEMORY && m.Stats.Sum(u => u.ParticipantCount) == 0);

            log.LogInformation($"Found '{shutDown.Count()}' machines to shut down");
            shutDown.AsParallel().ForAll(async m => await ionos.TurnMachineOff(m.Machine.Id, ionosDataCenter).ConfigureAwait(false));
        }

        internal static void ScaleMemoryDown(ILogger log, List<WorkloadMachineTuple> totalWorkload, IonosAPI ionos, string ionosDataCenter)
        {
            var scaleMemoryDown = totalWorkload.OrderByDescending(m => m.Workload.MemoryUtilization).Where(m => m.Workload.MemoryUtilization < MIN_ALLOWED_MEMORY_WORKLOAD
                               && m.Machine.Properties.Ram > DEFAULT_WORKER_MEMORY);
            log.LogInformation($"Found '{scaleMemoryDown.Count()}' machines to scale down");
            scaleMemoryDown.AsParallel().ForAll(async m =>
            {
                var machine = m.Machine;
                log.LogInformation($"Scale memory of machine '{machine.PrimaryIP}' down");
                var update = new IonosMachineUpdate { Ram = machine.Properties.Ram - 1024 };
                await ionos.UpdateMachines(machine.Id, ionosDataCenter, update).ConfigureAwait(false);
            });
        }

        internal static void ScaleCPUDown(ILogger log, List<WorkloadMachineTuple> totalWorkload, IonosAPI ionos, string ionosDataCenter)
        {
            var scaleCpuDown = totalWorkload.OrderByDescending(m => m.Workload.CPUUtilization).Where(m => m.Workload.CPUUtilization < MIN_ALLOWED_CPU_WORKLOAD
                               && m.Machine.Properties.Cores > DEFAULT_WORKER_CPU);
            log.LogInformation($"Found '{scaleCpuDown.Count()}' machines to scale down");
            scaleCpuDown.AsParallel().ForAll(async m =>
            {
                var machine = m.Machine;
                log.LogInformation($"Scale machine '{machine.PrimaryIP}' down");
                var update = new IonosMachineUpdate { Cores = machine.Properties.Cores - 1 };
                await ionos.UpdateMachines(machine.Id, ionosDataCenter, update).ConfigureAwait(false);
            });
        }

        internal static void ScaleMemoryUp(ILogger log, List<WorkloadMachineTuple> totalWorkload, IonosAPI ionos, string ionosDataCenter)
        {
            var dyingMemoryMachines = totalWorkload.Where(m => m.Workload.MemoryUtilization > MAX_ALLOWED_MEMORY_WORKLOAD && m.Machine.Properties.Ram + 1024 <= MAX_WORKER_MEMORY);
            log.LogInformation($"Found '{dyingMemoryMachines.Count()}' machines to scale memory up");
            dyingMemoryMachines.AsParallel().ForAll(async m =>
            {
                var machine = m.Machine;
                log.LogInformation($"Scale memory of '{machine.PrimaryIP}' up");
                var update = new IonosMachineUpdate { Ram = machine.Properties.Ram + 1024 };
                await ionos.UpdateMachines(machine.Id, ionosDataCenter, update).ConfigureAwait(false);
            });
        }
        internal static void ScaleCPUUp(ILogger log, List<WorkloadMachineTuple> totalWorkload, IonosAPI ionos, string ionosDataCenter)
        {
            var dyingCPUMachines = totalWorkload.Where(m => m.Workload.CPUUtilization > MAX_ALLOWED_CPU_WORKLOAD && m.Machine.Properties.Cores + 1 <= MAX_WORKER_CPU);
            log.LogInformation($"Found '{dyingCPUMachines.Count()}' machines to scale cpu up");
            dyingCPUMachines.AsParallel().ForAll(async m =>
            {
                var machine = m.Machine;
                log.LogInformation($"Scale cpu of '{machine.PrimaryIP}' up");
                var update = new IonosMachineUpdate { Cores = machine.Properties.Cores + 1 };
                await ionos.UpdateMachines(machine.Id, ionosDataCenter, update).ConfigureAwait(false);
            });
        }
    }
}
