using HPI.BBB.Autoscaler.Models.Ionos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HPI.BBB.Autoscaler.APIs
{
    class IonosAPI
    {
        private const string MESSAGE = "message";
        public readonly string MACHINE_START_URL = "https://api.ionos.com/cloudapi/v5/datacenters/{0}/servers/{1}/start";
        public readonly string MACHINE_END_URL = "https://api.ionos.com/cloudapi/v5/datacenters/{0}/servers/{1}/stop";
        private readonly string MACHINE_GET_URL = "https://api.ionos.com/cloudapi/v5/datacenters/{0}/servers/{1}";
        public readonly string MACHINE_GETALL_URL = "https://api.ionos.com/cloudapi/v5/datacenters/{0}/servers";

        public readonly string NIC_GET_URL = "https://api.ionos.com/cloudapi/v5/datacenters/{0}/servers/{1}/nics/{2}";
        public readonly string DATACENTER_GET_URL = "https://api.ionos.com/cloudapi/v5/datacenters/{0}/servers/{1}/start";
        private readonly ILogger log;
        private static HttpClient httpClient;

        public IonosAPI(ILogger log, string user, string pw)
        {
            this.log = log;
            USER = user;
            PW = pw;
        }

        public string USER { get; }
        public string PW { get; }

        public async Task TurnMachineOff(string machineId, string dataCenterId)
        {
            if (string.IsNullOrEmpty(machineId))
            {
                throw new ArgumentException(MESSAGE, nameof(machineId));
            }

            if (string.IsNullOrEmpty(dataCenterId))
            {
                throw new ArgumentException(MESSAGE, nameof(dataCenterId));
            }

            HttpClient client = SetupHttpClient();
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, MACHINE_END_URL, dataCenterId, machineId));
            var result = await client.PostAsync(uri, null).ConfigureAwait(false);

            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task TurnMachineOn(string machineId, string dataCenterId)
        {
            if (string.IsNullOrEmpty(machineId))
            {
                throw new ArgumentException(MESSAGE, nameof(machineId));
            }

            if (string.IsNullOrEmpty(dataCenterId))
            {
                throw new ArgumentException(MESSAGE, nameof(dataCenterId));
            }

            HttpClient client = SetupHttpClient();
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, MACHINE_START_URL, dataCenterId, machineId));
            var result = await client.PostAsync(uri, null).ConfigureAwait(false);

            string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task UpdateMachines(string machineId, string dataCenterId, IonosMachineUpdate update)
        {
            if (string.IsNullOrEmpty(machineId))
            {
                throw new ArgumentException("message", nameof(machineId));
            }

            if (string.IsNullOrEmpty(dataCenterId))
            {
                throw new ArgumentException(MESSAGE, nameof(dataCenterId));
            }

            if (update is null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            HttpClient client = SetupHttpClient();
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, MACHINE_GET_URL, dataCenterId, machineId));

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
            serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            string json = JsonConvert.SerializeObject(update, serializerSettings);
            using var content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var result = await client.PatchAsync(uri, content).ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
                await RetryPatchAsync(client, uri, content).ConfigureAwait(false);
        }

        public async Task<IonosObjectCollection[]> GetAllMachines(string dataCenterId)
        {
            if (string.IsNullOrEmpty(dataCenterId))
            {
                throw new ArgumentException(MESSAGE, nameof(dataCenterId));
            }

            HttpClient client = SetupHttpClient();
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, MACHINE_GETALL_URL, dataCenterId));

            try
            {
                var result = await client.GetAsync(uri).ConfigureAwait(false);

                if ((int)result.StatusCode == 429)
                    result = await RetryGetAsync(client, uri).ConfigureAwait(false);

                string json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                IonosObjectCollection machine = JsonConvert.DeserializeObject<IonosObjectCollection>(json);

                return machine.Items;
            }
            catch (Exception e)
            {
                log.LogError(e, "Uri: {0}", uri);
                throw;
            }
        }




        public async Task<IonosMachine> GetMachineDetails(string machineId, string dataCenterId)
        {
            if (string.IsNullOrEmpty(machineId))
            {
                throw new ArgumentException(MESSAGE, nameof(machineId));
            }

            if (string.IsNullOrEmpty(dataCenterId))
            {
                throw new ArgumentException(MESSAGE, nameof(dataCenterId));
            }


            HttpClient client = SetupHttpClient();

            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, MACHINE_GET_URL + "?depth=2", dataCenterId, machineId));


            try
            {
                var result = await client.GetAsync(uri).ConfigureAwait(false);

                if ((int)result.StatusCode == 429)
                    result = await RetryGetAsync(client, uri).ConfigureAwait(false);

                string json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                IonosMachine machine = JsonConvert.DeserializeObject<IonosMachine>(json);

                return machine;
            }
            catch (Exception e)
            {
                log.LogError(e, "Uri: {0}", uri);
                throw;
            }
        }




        public async Task<IonosNIC> GetLAN(string machineId, string nicId, string dataCenterId)
        {
            if (string.IsNullOrEmpty(machineId))
            {
                throw new ArgumentException(MESSAGE, nameof(machineId));
            }

            if (string.IsNullOrEmpty(dataCenterId))
            {
                throw new ArgumentException(MESSAGE, nameof(dataCenterId));
            }


            HttpClient client = SetupHttpClient();

            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, NIC_GET_URL, dataCenterId, machineId, nicId));
            var result = await client.GetAsync(uri).ConfigureAwait(false);

            if ((int)result.StatusCode == 429)
                result = await RetryGetAsync(client, uri).ConfigureAwait(false);

            string json = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            IonosNIC nic = JsonConvert.DeserializeObject<IonosNIC>(json);

            return nic;
        }

        private HttpClient SetupHttpClient()
        {
            if (httpClient == null)
            {
                httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{USER}:{PW}")));
            }
            return httpClient;
        }


        private async Task<HttpResponseMessage> RetryPatchAsync(HttpClient client, Uri uri, HttpContent content)
        {
            HttpResponseMessage result = null;

            for (int i = 0; i < 3; i++)
            {
                result = await client.PatchAsync(uri, content).ConfigureAwait(false);
                string error = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (result.IsSuccessStatusCode)
                    break;

                Random random = new Random(DateTime.Now.Millisecond);
                int time = random.Next(60000, 90000);
                Thread.Sleep(time);
            }

            if (!result.IsSuccessStatusCode)
                throw new OperationCanceledException(await result.Content.ReadAsStringAsync());

            return result;
        }

        private async Task<HttpResponseMessage> RetryGetAsync(HttpClient client, Uri uri)
        {
            HttpResponseMessage result = null;

            for (int i = 0; i < 3; i++)
            {
                result = await client.GetAsync(uri).ConfigureAwait(false);

                if (result.IsSuccessStatusCode)
                    break;

                Random random = new Random(DateTime.Now.Millisecond);
                int time = random.Next(60000, 90000);
                Thread.Sleep(time);
            }

            if (!result.IsSuccessStatusCode)
                throw new OperationCanceledException(await result.Content.ReadAsStringAsync());

            return result;
        }

    }
}
