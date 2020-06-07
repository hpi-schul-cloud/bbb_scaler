using HPI.BBB.Autoscaler.Models;
using HPI.BBB.Autoscaler.Models.Prometheus;
using HPI.BBB.Autoscaler.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HPI.BBB.Autoscaler.APIs
{
    class NodeExporterAPI
    {



        public static async Task<Workload> GetWorkLoadAsync(ILogger log, string ip, string graphanaKey, string nodeExporterUserName, string nodeExporterPassword)
        {
            log.LogInformation($"Get workload for '{ip}'");

            HttpClient client = new HttpClient();

            Workload workload = new Workload();
            workload.CPUUtilization = await GetCPUUtilization(client, ip, graphanaKey) ??
                float.Parse(ConfigReader.GetValue("MIN_ALLOWED_CPU_WORKLOAD", "DEFAULT", "MIN_ALLOWED_CPU_WORKLOAD"));

            log.LogInformation($"Get Metrics of '{ip}'");
            string url = $"https://{ip}:9100/metrics";

            // TODO Add Certificate Validation
            log.LogInformation($"Turn cert validation off of '{ip}'");
            using var httpClientHandler = new HttpClientHandler();
            client = new HttpClient(httpClientHandler);

            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{nodeExporterUserName}:{nodeExporterPassword}")));

            HttpResponseMessage result = null;

            try
            {
                result = await client.GetAsync(new Uri(url)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                log.LogError(e, "ip: {0}, url: {1}", ip, url);
                throw;
            }

            string metrics = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            log.LogInformation($"Parse cpu seconds of '{ip}'");

            log.LogInformation($"Parse total memory of '{ip}'");
            string rgx = @"(?<tm>node_memory_MemTotal_bytes *(?<totalmem>[e+\.\d]+))";

            Regex totalMem = new Regex(rgx);
            var memtotalRes = totalMem.Match(metrics);

            workload.TotalMemory = long.Parse(string.IsNullOrEmpty(memtotalRes.Groups["totalmem"].Value) ? "1" : memtotalRes.Groups["totalmem"].Value,
                 NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
            log.LogInformation($"Total memory of '{ip}' is {workload.TotalMemory}");

            log.LogInformation($"Parse available memory of '{ip}'");
            rgx = @"(node_memory_MemAvailable_bytes *(?<availmem>[e+\.\d]+))";
            Regex freeMem = new Regex(rgx);
            var freeMemRes = freeMem.Match(metrics);

            workload.AvailableMemory = long.Parse(string.IsNullOrEmpty(freeMemRes.Groups["availmem"]?.Value) ? "1" : freeMemRes.Groups["availmem"]?.Value,
                NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
            log.LogInformation($"Available memory of '{ip}' is {workload.AvailableMemory}");

            log.LogInformation($"Calculate used memory of '{ip}'");
            workload.UsedMemory = workload.TotalMemory - workload.AvailableMemory;
            log.LogInformation($"Used memory of '{ip}' is {workload.UsedMemory}");

            log.LogInformation($"Calculate memory utilization of '{ip}'");
            workload.MemoryUtilization = workload.UsedMemory / (float)workload.TotalMemory;
            log.LogInformation($"Memory utilization of '{ip}' is {workload.MemoryUtilization}");

            return workload;
        }

        private static async Task<float?> GetCPUUtilization(HttpClient client, string ip, string graphanaKey)
        {
            //100 - (avg by(mode)(irate(node_cpu_seconds_total{ mode = "idle"}[1m]))*100)
            //(avg by(instance)(irate(node_cpu_seconds_total{instance = "{ip},job="bbb"}[5m])))
            string cpuQuery = $"1 - (avg by (instance) (irate(node_cpu_seconds_total{{instance=\"{ip}:9100\", mode=\"idle\"}}[5m])))";
            string url = $"https://jitsi.dev.messenger.schule/grafana/api/datasources/proxy/1/api/v1/query?query={cpuQuery}";
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", graphanaKey);
            var result = await client.GetAsync(url).ConfigureAwait(false);
            string json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            var promResult = JsonConvert.DeserializeObject<PrometheusResult>(json);

            return promResult.Data.Result
                 //If result then there is only one
                 .FirstOrDefault()?
                 //First value is unix time, second is result
                 .Value?.LastOrDefault();
        }
    }
}
