using HPI.BBB.Autoscaler.APIs;
using HPI.BBB.Autoscaler.Models;
using HPI.BBB.Autoscaler.Models.Ionos;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace HPI.BBB.Autoscaler.Utils
{
    public class IonosHelper
    {

        internal static async Task<List<IonosMachine>> GetMachines(ILogger log, IonosAPI ionos, string dataCenterId)
        {
            var ml = await ionos.GetAllMachines(dataCenterId).ConfigureAwait(false);
            //Get IPs
            log.LogInformation("Get machine details");
            var machines = ml.AsParallel().Select(async m => await ionos.GetMachineDetails(m.Id, dataCenterId).ConfigureAwait(false))
                .Select(m => m.Result)
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
                //Filter for BBB
                .Where(m => m.Properties.Name.ToUpperInvariant().Contains("BBB", StringComparison.InvariantCultureIgnoreCase)
                    && m.PrimaryIP != null).ToList();
            return machines;
        }

        internal static async Task<List<MachineDataCenterTuple>> GetMachinesByDataCenter(ILogger log, IonosAPI ionos, string[] dataCenterIds)
        {
            var result = new List<MachineDataCenterTuple>();
            foreach (string id in dataCenterIds)
            {
                var machines = await IonosHelper.GetMachines(log, ionos, id);
                foreach (IonosMachine m in machines)
                {
                    result.Add(new MachineDataCenterTuple(id, m));
                }
            }
            return result;
        }
    }
}
