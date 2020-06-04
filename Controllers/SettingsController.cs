using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HPI.BBB.Autoscaler.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HPI.BBB.Autoscaler.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly string SubscriptionKey = Environment.GetEnvironmentVariable("SUBSCRIPTION_KEY");
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ILogger<SettingsController> logger)
        {
            _logger = logger;
        }

        public static AutoScalingSettings Settings { get; private set; }

        [HttpPost]
        public IActionResult Post([FromBody]AutoScalingSettings settings)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SUBSCRIPTION_KEY")))
                return NotFound();

            if (!Request.Headers.ContainsKey("Subscription-Key"))
                return NotFound();

            if (Request.Headers["Subscription-Key"] != Environment.GetEnvironmentVariable("SUBSCRIPTION_KEY"))
                return NotFound();

            Settings = settings;

            return Ok();
        }
    }
}
