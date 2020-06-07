using HPI.BBB.Autoscaler.Models.BBB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HPI.BBB.Autoscaler.APIs
{
    public class BBBAPI
    {
        private ILogger log;

        public string BBBKey { get; }

        public BBBAPI(ILogger logger, string bbbKey)
        {
            this.log = logger;
            BBBKey = bbbKey;
        }

        private string CreateChecksumParameter(string apiName, string parameter)
        {
            string encodedParameters = string.Concat(apiName, parameter, BBBKey);

            
            var hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(encodedParameters));

            StringBuilder builder = new StringBuilder();
            hash.Select(p => builder.Append(p.ToString("x2"))).ToList();

            string checksum = builder.ToString();
            return parameter += parameter?.Length > 0 ? "&checksum=" + checksum : "checksum=" + checksum;
        }

        public async Task<List<Meeting>> GetMeetingsAsync(string baseIP)
        {
            if (string.IsNullOrEmpty(baseIP))
            {
                throw new ArgumentException("message", nameof(baseIP));
            }

            log.LogInformation($"Build checksum bbb of '{baseIP}'");
            string uri = $"https://{baseIP}/bigbluebutton/api/getMeetings?";
            uri += CreateChecksumParameter("getMeetings", "");

            // TODO Add Certificate Validation
            log.LogInformation($"Turn cert validation off of '{baseIP}'");
            using var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

            using HttpClient httpClient = new HttpClient(httpClientHandler);
            log.LogInformation($"Get meeting stats of '{baseIP}'");
            var result = await httpClient.GetAsync(uri);
            string xmlText = await result.Content.ReadAsStringAsync();
            XmlSerializer serializer = new XmlSerializer(typeof(GetMeetingsResponse));
            using (StringReader reader = new StringReader(xmlText))
            {
                var response = (GetMeetingsResponse)(serializer.Deserialize(reader));
                return response.Meetings.Meeting;
            }
        }
    }
}
