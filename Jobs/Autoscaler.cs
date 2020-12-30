using HPI.BBB.Autoscaler.APIs;
using HPI.BBB.Autoscaler.Models;
using HPI.BBB.Autoscaler.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HPI.BBB.Autoscaler.Jobs
{
    public class Autoscaler : IHostedService
    {
        private static readonly int WAITINGTIME = int.Parse(ConfigReader.GetValue("WAITINGTIME", "DEFAULT", "WAITINGTIME"), CultureInfo.InvariantCulture);
        private static readonly float MAX_ALLOWED_MEMORY_WORKLOAD = float.Parse(ConfigReader.GetValue("MAX_ALLOWED_MEMORY_WORKLOAD", "DEFAULT", "MAX_ALLOWED_MEMORY_WORKLOAD"), CultureInfo.InvariantCulture);
        private static readonly float MAX_ALLOWED_CPU_WORKLOAD = float.Parse(ConfigReader.GetValue("MAX_ALLOWED_CPU_WORKLOAD", "DEFAULT", "MAX_ALLOWED_CPU_WORKLOAD"), CultureInfo.InvariantCulture);

        public Autoscaler(ILogger<Autoscaler> logger)
        {
            log = logger;
        }

        private ILogger<Autoscaler> log { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await StartJob(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }

        async Task StartJob(CancellationToken cancellationToken)
        {
            log.LogInformation("Start autoscaler job");

            string ionosUser = Environment.GetEnvironmentVariable("IONOS_USER");
            string ionosPass = Environment.GetEnvironmentVariable("IONOS_PASS");
            string[] ionosDataCenterIds = Environment.GetEnvironmentVariable("IONOS_DATACENTER").Split(",");
            string bbbKey = Environment.GetEnvironmentVariable("BBB_PASS");
            string graphanaKey = Environment.GetEnvironmentVariable("GRAFANA_PASS");
            string neUser = Environment.GetEnvironmentVariable("NE_BASIC_AUTH_USER");
            string nePass = Environment.GetEnvironmentVariable("NE_BASIC_AUTH_PASS");


            if (string.IsNullOrEmpty(ionosUser) || string.IsNullOrEmpty(ionosPass) || ionosDataCenterIds == null || ionosDataCenterIds.Any( id => string.IsNullOrEmpty(id)))
            {
                log.LogInformation("Load dev environment variables");

                ionosUser = ConfigReader.GetConfigurationValue("IONOS", "USER");
                ionosPass = ConfigReader.GetConfigurationValue("IONOS", "PASS");
                ionosDataCenterIds = ConfigReader.GetConfigurationValue("IONOS", "DATACENTER").Split(",");
                bbbKey = ConfigReader.GetConfigurationValue("BBB", "PASS");
                graphanaKey = ConfigReader.GetConfigurationValue("GRAFANA", "PASS");
                neUser = ConfigReader.GetConfigurationValue("NODE_EXPORTER", "USER");
                nePass = ConfigReader.GetConfigurationValue("NODE_EXPORTER", "PASS");
            }

            log.LogInformation("Init Ionos API");
            IonosAPI ionos = new IonosAPI(log, ionosUser, ionosPass);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    ScalingHelper.EnsureMinimumRunningServers(log, ionos, ionosDataCenterIds);

                    log.LogInformation("Get machines");
                    var machines = await IonosHelper.GetMachinesByDataCenter(log, ionos, ionosDataCenterIds);
                    var runningMachines = machines.Where(m => m.Machine.Properties.VmState == "RUNNING");
                    var totalWorkload = runningMachines.AsParallel()
                        .Select(async m => new WorkloadMachineTuple(m.DataCenter, m.Machine, await NodeExporterAPI.GetWorkLoadAsync(log, m.Machine.SecondaryIP ?? m.Machine.PrimaryIP, graphanaKey, neUser, nePass).ConfigureAwait(false)))
                        .Select(m => m.Result).ToList();

                    float avarageMemoryWorkload = totalWorkload.Average(m => m.Workload.MemoryUtilization);
                    float avarageCPUWorkload = totalWorkload.Average(m => m.Workload.CPUUtilization);

                    log.LogInformation($"Average workload is '{avarageMemoryWorkload}'");
                    log.LogInformation($"Max allowed workload is '{MAX_ALLOWED_MEMORY_WORKLOAD}'");
                    if (avarageMemoryWorkload >= MAX_ALLOWED_MEMORY_WORKLOAD || avarageCPUWorkload >= MAX_ALLOWED_CPU_WORKLOAD)
                    {
                        log.LogInformation("Wake up sleeping machine");
                        var sleepingMachines = machines.Where(m => !runningMachines.Contains(m));
                        var toBeTurnedOn = sleepingMachines.FirstOrDefault();

                        if (toBeTurnedOn != null)
                            await ionos.TurnMachineOn(toBeTurnedOn.Machine.Id, toBeTurnedOn.DataCenter).ConfigureAwait(false);
                    }
                    
                    BBBAPI bbb = new BBBAPI(log, bbbKey);
                    ScalingHelper.ShutDown(log, totalWorkload, bbb, ionos);

                    //Scale all machines down that have addional memory received
                    ScalingHelper.ScaleMemoryDown(log, totalWorkload, bbb, ionos);

                    //Scale all machines down that have addional cpu received
                    ScalingHelper.ScaleCPUDown(log, totalWorkload, bbb, ionos);

                    //Scale all machines up thats memory are over the max workload threshold
                    ScalingHelper.ScaleMemoryUp(log, totalWorkload, ionos);

                    //Scale all machines up thats cpu utilization is over the max workload threshold
                    ScalingHelper.ScaleCPUUp(log, totalWorkload, ionos);

                    log.LogInformation($"Sleep for '{WAITINGTIME}' milliseconds ('{WAITINGTIME / 60000} minutes')");
                    
                    Thread.Sleep(WAITINGTIME);
                }
                catch (Exception e)
                {
                    log.LogError(e, "Exeption thrown:");

                    log.LogInformation($"Sleep for '{WAITINGTIME}' milliseconds ('{WAITINGTIME / 60000} minutes')");
                    Thread.Sleep(WAITINGTIME);
                }
            }
        }

    }
}
