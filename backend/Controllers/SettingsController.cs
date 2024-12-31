using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

namespace QuantumCrossScripting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // In-memory settings for demonstration; this could be replaced with a DB or a config file in production
        private static SettingsModel _settings = new SettingsModel
        {
            Setting1 = "Value1",
            Setting2 = "Value2"
        };

        public SettingsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetSettings()
        {
            // Return the current settings
            return Ok(_settings);
        }

        [HttpPost]
        public IActionResult UpdateSettings([FromBody] SettingsModel model)
        {
            // Validate the incoming model
            if (model == null || string.IsNullOrWhiteSpace(model.Setting1) || string.IsNullOrWhiteSpace(model.Setting2))
            {
                return BadRequest("Invalid settings data.");
            }

            // Optionally, perform more advanced validation (e.g., check if settings values meet certain criteria)
            if (model.Setting1.Length > 100 || model.Setting2.Length > 100)
            {
                return BadRequest("Settings values cannot be longer than 100 characters.");
            }

            // Update settings (this could be saving to a database or file in a real-world application)
            _settings.Setting1 = model.Setting1;
            _settings.Setting2 = model.Setting2;

            // Here you could persist the settings to a database or file if required
            // For example, save to appsettings.json or a settings database.

            // Return a success message
            return Ok(new { message = "Settings updated successfully" });
        }
    }

    // Settings model
    public class SettingsModel
    {
        public string Setting1 { get; set; }
        public string Setting2 { get; set; }
    }
}
