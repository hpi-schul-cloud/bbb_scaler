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
            string ionosDataCenter = Environment.GetEnvironmentVariable("IONOS_DATACENTER");
            string bbbKey = Environment.GetEnvironmentVariable("BBB_PASS");
            string graphanaKey = Environment.GetEnvironmentVariable("GRAPHANA_PASS");
            string neUser = Environment.GetEnvironmentVariable("NE_BASIC_AUTH_USER");
            string nePass = Environment.GetEnvironmentVariable("NE_BASIC_AUTH_PASS");


            if (string.IsNullOrEmpty(ionosUser) || string.IsNullOrEmpty(ionosPass) || string.IsNullOrEmpty(ionosDataCenter))
            {
                log.LogInformation("Load dev environment variables");

                ionosUser = ConfigReader.GetConfigurationValue("IONOS", "USER");
                ionosPass = ConfigReader.GetConfigurationValue("IONOS", "PASS");
                ionosDataCenter = ConfigReader.GetConfigurationValue("IONOS", "DATACENTER");
                bbbKey = ConfigReader.GetConfigurationValue("BBB", "PASS");
                graphanaKey = ConfigReader.GetConfigurationValue("GRAPHANA", "PASS");
                neUser = ConfigReader.GetConfigurationValue("NODE_EXPORTER", "USER");
                nePass = ConfigReader.GetConfigurationValue("NODE_EXPORTER", "PASS");
            }

            log.LogInformation("Init Ionos API");
            IonosAPI ionos = new IonosAPI(log, ionosUser, ionosPass);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {

                    log.LogInformation("Get internet ip");

                    using var web = new WebClient();
                    string externalip = web.DownloadString("http://icanhazip.com")
                                                       .Replace("\n", "", true, CultureInfo.InvariantCulture)
                                                       .Replace("\r", "", true, CultureInfo.InvariantCulture)
                                                       .Trim();

#if DEBUG
                    externalip = "81.173.115.126";
#endif

                    //Get all machines 
                    log.LogInformation("Get machines");
                    var ml = await ionos.GetAllMachines(ionosDataCenter).ConfigureAwait(false);

                    //Get IPs
                    log.LogInformation("Get machine details");
                    var machines = ml.AsParallel().Select(async m => await ionos.GetMachineDetails(m.Id, ionosDataCenter).ConfigureAwait(false)).Select(m => m.Result)
                        .Select(async m =>
                        {

                            if (m.Entities.Nics.Items.Count > 0)
                                m.PrimaryIP = m.Entities.Nics.Items
                                .FirstOrDefault(p => p.Properties.Name.ToUpperInvariant() == "public".ToUpperInvariant())
                                .Properties.Ips.FirstOrDefault();


                            if (m.Entities.Nics.Items.Count > 1)
                                m.SecondaryIP = m.Entities.Nics.Items
                                .FirstOrDefault(p => p.Properties.Name.ToUpperInvariant() != "public".ToUpperInvariant())
                                .Properties.Ips.FirstOrDefault();

                            return m;
                        })
                        .Select(m => m.Result)
                        //Filter for Scalelite
                        .Where(m => !m.Properties.Name.ToUpperInvariant().Contains("SCALELITE", StringComparison.InvariantCultureIgnoreCase)
                            && m.PrimaryIP != null)
                        .ToList();


                    log.LogInformation("Get running machines");
                    var runningMachines = machines.Where(m => m.Properties.VmState == "RUNNING").ToList();

                    log.LogInformation("Get workload of machines");
                    var totalWorkload = runningMachines.AsParallel()
                        .Select(async m => new WorkloadMachineTuple(m, await NodeExporterAPI.GetWorkLoadAsync(log, m.SecondaryIP ?? m.PrimaryIP, graphanaKey, neUser, nePass).ConfigureAwait(false)))
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
                            await ionos.TurnMachineOn(toBeTurnedOn.Id, ionosDataCenter).ConfigureAwait(false);
                    }

                    //Shut all machines down that have no more memory to reduce an broken the threshold
                    log.LogInformation("Connect to host and get current sessions");
                    BBBAPI bbb = new BBBAPI(log, bbbKey);
                    ScalingHelper.ShutDown(log, totalWorkload, bbb, ionos, ionosDataCenter);

                    //Scale all machines down that have addional memory received
                    ScalingHelper.ScaleMemoryDown(log, totalWorkload, ionos, ionosDataCenter);

                    //Scale all machines down that have addional cpu received
                    ScalingHelper.ScaleCPUDown(log, totalWorkload, ionos, ionosDataCenter);

                    //Scale all machines up thats memory are over the max workload threshold
                    ScalingHelper.ScaleMemoryUp(log, totalWorkload, ionos, ionosDataCenter);

                    //Scale all machines up thats cpu utilization is over the max workload threshold
                    ScalingHelper.ScaleCPUUp(log, totalWorkload, ionos, ionosDataCenter);

                    log.LogInformation($"Sleep for '{WAITINGTIME}' milliseconds ('{WAITINGTIME / 60000} minutes')");
                    Thread.Sleep(WAITINGTIME);
                }
                catch (Exception e)
                {
                    log.LogError(e, "Exepthion thrown:");

                    log.LogInformation($"Sleep for '{WAITINGTIME}' milliseconds ('{WAITINGTIME / 60000} minutes')");
                    Thread.Sleep(WAITINGTIME);
                }
            }
        }

    }
}
