using HPI.BBB.Autoscaler.APIs;
using HPI.BBB.Autoscaler.Models;
using HPI.BBB.Autoscaler.Models.Ionos;
using HPI.BBB.Autoscaler.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HPI.BBB.Autoscaler.Utils
{
    public class ScalingHelper
    {

        private static readonly float MAX_ALLOWED_MEMORY_WORKLOAD = float.Parse(ConfigReader.GetValue("MAX_ALLOWED_MEMORY_WORKLOAD", "DEFAULT", "MAX_ALLOWED_MEMORY_WORKLOAD"), CultureInfo.InvariantCulture);
        private static readonly float MAX_ALLOWED_CPU_WORKLOAD = float.Parse(ConfigReader.GetValue("MAX_ALLOWED_CPU_WORKLOAD", "DEFAULT", "MAX_ALLOWED_CPU_WORKLOAD"), CultureInfo.InvariantCulture);
        private static readonly float MIN_ALLOWED_MEMORY_WORKLOAD = float.Parse(ConfigReader.GetValue("MIN_ALLOWED_MEMORY_WORKLOAD", "DEFAULT", "MIN_ALLOWED_MEMORY_WORKLOAD"), CultureInfo.InvariantCulture);
        private static readonly float MIN_ALLOWED_CPU_WORKLOAD = float.Parse(ConfigReader.GetValue("MIN_ALLOWED_CPU_WORKLOAD", "DEFAULT", "MIN_ALLOWED_CPU_WORKLOAD"), CultureInfo.InvariantCulture);
        private static readonly int MAX_WORKER_MEMORY = int.Parse(ConfigReader.GetValue("MAX_WORKER_MEMORY", "DEFAULT", "MAX_WORKER_MEMORY"), CultureInfo.InvariantCulture);
        private static readonly int DEFAULT_WORKER_MEMORY = int.Parse(ConfigReader.GetValue("DEFAULT_WORKER_MEMORY", "DEFAULT", "DEFAULT_WORKER_MEMORY"), CultureInfo.InvariantCulture);
        private static readonly int MAX_WORKER_CPU = int.Parse(ConfigReader.GetValue("MAX_WORKER_CPU", "DEFAULT", "MAX_WORKER_CPU"), CultureInfo.InvariantCulture);
        private static readonly int DEFAULT_WORKER_CPU = int.Parse(ConfigReader.GetValue("DEFAULT_WORKER_CPU", "DEFAULT", "DEFAULT_WORKER_CPU"), CultureInfo.InvariantCulture);
        private static readonly int MAX_SERVER_INCREASE = int.Parse(ConfigReader.GetValue("MAX_SERVER_INCREASE", "DEFAULT", "MAX_SERVER_INCREASE"), CultureInfo.InvariantCulture);

        internal static void ShutDown(ILogger log, List<WorkloadMachineTuple> totalWorkload, BBBAPI bbb, IonosAPI ionos)
        {
            var shutDown = totalWorkload
                        .OrderByDescending(m => m.Workload.MemoryUtilization)
                        .ThenByDescending(m => m.Workload.CPUUtilization)
                        .Skip(TimeHelper.GetMinimumActiveMachines())
                        .Select(async m => new MachineWorkloadStatsTuple(m.DataCenter, m.Machine, m.Workload, await bbb.GetMeetingsAsync(m.Machine.PrimaryIP).ConfigureAwait(false)))
                        .Select(res => res.Result)
                        .Where(m => (m.Workload.MemoryUtilization < MIN_ALLOWED_MEMORY_WORKLOAD ||
                        m.Workload.CPUUtilization < MIN_ALLOWED_CPU_WORKLOAD) && m.Machine.Properties.Cores <= DEFAULT_WORKER_CPU &&
                        m.Machine.Properties.Ram <= DEFAULT_WORKER_MEMORY && m.Stats.Sum(u => u.ParticipantCount) == 0).ToList();

            log.LogInformation($"Found '{shutDown.Count}' machines to shut down");
            shutDown.AsParallel().ForAll(async m => await ionos.TurnMachineOff(m.Machine.Id, m.DataCenter).ConfigureAwait(false));
        }

        internal static async void EnsureMinimumRunningServers(ILogger log, IonosAPI ionos, string[] ionosDataCenterIds)
        {
            var machines = await IonosHelper.GetMachinesByDataCenter(log, ionos, ionosDataCenterIds);
            var runningMachines = machines.Where(m => m.Machine.Properties.VmState != "SHUTOFF").ToList();
            var toStartMachines = machines.Where(m => m.Machine.Properties.VmState == "SHUTOFF")
                .Take(MAX_SERVER_INCREASE)
                .ToList();
            if (toStartMachines.Count > 0)
            {
                log.LogInformation($"Found '{toStartMachines.Count}' machines to start");
                toStartMachines.AsParallel()
                    .ForAll(async m => await ionos.TurnMachineOn(m.Machine.Id, m.DataCenter).ConfigureAwait(false));
            }
            else
            {
                log.LogInformation("Found '0' machines to start");
            }
        }

        internal static void ScaleMemoryDown(ILogger log, List<WorkloadMachineTuple> totalWorkload, BBBAPI bbb, IonosAPI ionos)
        {
            var scaleMemoryDown = totalWorkload
                        .OrderByDescending(m => m.Workload.MemoryUtilization)
                        .Select(async m => new MachineWorkloadStatsTuple(m.DataCenter, m.Machine, m.Workload, await bbb.GetMeetingsAsync(m.Machine.PrimaryIP).ConfigureAwait(false)))
                        .Select(res => res.Result)
                        .Where(m => m.Workload.MemoryUtilization < MIN_ALLOWED_MEMORY_WORKLOAD
                               && m.Machine.Properties.Ram > DEFAULT_WORKER_MEMORY && m.Stats.Sum(u => u.ParticipantCount) == 0).ToList();
            log.LogInformation($"Found '{scaleMemoryDown.Count}' machines to scale memory down");
            scaleMemoryDown.AsParallel().ForAll(async m =>
            {
                var machine = m.Machine;
                log.LogInformation($"Scale memory of machine '{machine.PrimaryIP}' down");
                var update = new IonosMachineUpdate { Ram = machine.Properties.Ram - 1024 };
                await ionos.UpdateMachines(machine.Id, m.DataCenter, update).ConfigureAwait(false);
            });
        }

        internal static void ScaleCPUDown(ILogger log, List<WorkloadMachineTuple> totalWorkload, BBBAPI bbb, IonosAPI ionos)
        {
            var scaleCpuDown = totalWorkload.OrderByDescending(m => m.Workload.CPUUtilization)
                        .Select(async m => new MachineWorkloadStatsTuple(m.DataCenter, m.Machine, m.Workload, await bbb.GetMeetingsAsync(m.Machine.PrimaryIP).ConfigureAwait(false)))
                        .Select(res => res.Result)
                        .Where(m => m.Workload.CPUUtilization < MIN_ALLOWED_CPU_WORKLOAD
                               && m.Machine.Properties.Cores > DEFAULT_WORKER_CPU && m.Stats.Sum(u => u.ParticipantCount) == 0).ToList();
            log.LogInformation($"Found '{scaleCpuDown.Count}' machines to scale CPU down");
            scaleCpuDown.AsParallel().ForAll(async m =>
            {
                var machine = m.Machine;
                log.LogInformation($"Scale machine '{machine.PrimaryIP}' down");
                var update = new IonosMachineUpdate { Cores = machine.Properties.Cores - 1 };
                await ionos.UpdateMachines(machine.Id, m.DataCenter, update).ConfigureAwait(false);
            });
        }

        internal static void ScaleMemoryUp(ILogger log, List<WorkloadMachineTuple> totalWorkload, IonosAPI ionos)
        {
            var dyingMemoryMachines = totalWorkload.Where(m => m.Workload.MemoryUtilization > MAX_ALLOWED_MEMORY_WORKLOAD && m.Machine.Properties.Ram + 1024 <= MAX_WORKER_MEMORY).ToList();
            log.LogInformation($"Found '{dyingMemoryMachines.Count}' machines to scale memory up");
            dyingMemoryMachines.AsParallel().ForAll(async m =>
            {
                var machine = m.Machine;
                log.LogInformation($"Scale memory of '{machine.PrimaryIP}' up");
                var update = new IonosMachineUpdate { Ram = machine.Properties.Ram + 1024 };
                await ionos.UpdateMachines(machine.Id, m.DataCenter, update).ConfigureAwait(false);
            });
        }
        internal static void ScaleCPUUp(ILogger log, List<WorkloadMachineTuple> totalWorkload, IonosAPI ionos)
        {
            var dyingCPUMachines = totalWorkload.Where(m => m.Workload.CPUUtilization > MAX_ALLOWED_CPU_WORKLOAD && m.Machine.Properties.Cores + 1 <= MAX_WORKER_CPU).ToList();
            log.LogInformation($"Found '{dyingCPUMachines.Count}' machines to scale CPU up");
            dyingCPUMachines.AsParallel().ForAll(async m =>
            {
                var machine = m.Machine;
                log.LogInformation($"Scale cpu of '{machine.PrimaryIP}' up");
                var update = new IonosMachineUpdate { Cores = machine.Properties.Cores + 1 };
                await ionos.UpdateMachines(machine.Id, m.DataCenter, update).ConfigureAwait(false);
            });
        }
    }
}
