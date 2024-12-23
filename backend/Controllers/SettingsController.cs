using Microsoft.AspNetCore.Mvc;

namespace QuantumCrossScripting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SettingsController : ControllerBase
    {
        // In-memory settings for demonstration; this could be a database or a config file in production
        private static SettingsModel _settings = new SettingsModel
        {
            Setting1 = "Value1",
            Setting2 = "Value2"
        };

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
            if (model == null)
            {
                return BadRequest("Invalid settings data.");
            }

            // Update settings (this could be saving to a database or file in a real-world application)
            _settings.Setting1 = model.Setting1;
            _settings.Setting2 = model.Setting2;

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
