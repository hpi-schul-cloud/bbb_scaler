using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HPI.BBB.Autoscaler.Utils
{
    internal static class ConfigReader
    {
        internal static string GetValue(string envName, params string[] sections)
        {
            return Environment.GetEnvironmentVariable(envName) ?? GetConfigurationValue(sections);
        }

        internal static string GetConfigurationValue(params string[] sections)
        {
            if (sections is null || sections.Length == 0)
            {
                throw new ArgumentNullException(nameof(sections));
            }

            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            builder.AddUserSecrets<Program>();

            var Configuration = builder.Build();

            if (sections.Length == 1)
                return Configuration.GetSection(sections.First()).Value;

            return GetValue(sections.Skip(1).ToArray(), Configuration.GetSection(sections.First()));
        }

        private static string GetValue(string[] sections, IConfigurationSection section)
        {
            if (sections.Length == 0)
                return section.Value;

            return GetValue(sections.Skip(1).ToArray(), section.GetSection(sections.First()));
        }
    }
}
