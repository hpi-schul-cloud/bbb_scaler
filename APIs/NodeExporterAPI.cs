﻿using HPI.BBB.Autoscaler.Models;
using HPI.BBB.Autoscaler.Models.Prometheus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace HPI.BBB.Autoscaler.APIs
{
    class NodeExporterAPI
    {


        public static async System.Threading.Tasks.Task<Workload> GetWorkLoadAsync(ILogger log, string ip, string graphanaKey)
        {
            log.LogInformation($"Get workload for '{ip}'");

            using HttpClient client = new HttpClient();
            //100 - (avg by(mode)(irate(node_cpu_seconds_total{ mode = "idle"}[1m]))*100)
            string cpuQuery = $"100 - (avg by (instance) (irate(node_cpu_seconds_total{{instance=\"{ip}:9100\",job=\"bbb\",mode=\"idle\"}}[5m])) * 100)";
            string url = $"https://jitsi.dev.messenger.schule/grafana/api/datasources/proxy/1/api/v1/query?query={cpuQuery}";
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", graphanaKey);
            var result = await client.GetAsync(url).ConfigureAwait(false);
            string json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            var promResult = JsonConvert.DeserializeObject<PrometheusResult>(json);

            Workload workload = new Workload();

            workload.CPUUtilization = promResult.Data.Result
                //If result then there is only one
                .FirstOrDefault()?
                //First value is unix time, second is result
                .Value?.LastOrDefault() ?? 0;

            log.LogInformation($"Get Metrics of '{ip}'");
            url = $"http://{ip}:9100/metrics";
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
    }
}
