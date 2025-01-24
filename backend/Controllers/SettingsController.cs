using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Newtonsoft.Json;

namespace QuantumCrossScripting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SettingsController> _logger;
        private readonly string _settingsFilePath;

        // Constructor to inject IConfiguration and ILogger
        public SettingsController(IConfiguration configuration, ILogger<SettingsController> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Assuming settings are stored in a JSON file; can be replaced with DB or another storage
            _settingsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
        }

        [HttpGet]
        public IActionResult GetSettings()
        {
            try
            {
                var settings = ReadSettingsFromFile();
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading settings.");
                return StatusCode(500, "Error retrieving settings.");
            }
        }

        [HttpPost]
        public IActionResult UpdateSettings([FromBody] SettingsModel model)
        {
            // Validate the incoming model
            if (model == null || string.IsNullOrWhiteSpace(model.Setting1) || string.IsNullOrWhiteSpace(model.Setting2))
            {
                return BadRequest("Invalid settings data.");
            }

            // Additional validation
            if (model.Setting1.Length > 100 || model.Setting2.Length > 100)
            {
                return BadRequest("Settings values cannot be longer than 100 characters.");
            }

            try
            {
                // Update settings in memory (for demonstration purposes)
                var settings = new SettingsModel
                {
                    Setting1 = model.Setting1,
                    Setting2 = model.Setting2
                };

                // Save the settings to a JSON file
                SaveSettingsToFile(settings);

                _logger.LogInformation("Settings updated successfully.");
                return Ok(new { message = "Settings updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settings.");
                return StatusCode(500, "Error updating settings.");
            }
        }

        [HttpDelete("reset")]
        public IActionResult ResetSettings()
        {
            try
            {
                if (System.IO.File.Exists(_settingsFilePath))
                {
                    System.IO.File.Delete(_settingsFilePath);
                    _logger.LogInformation("Settings reset to default successfully.");
                    return Ok(new { message = "Settings reset to default successfully." });
                }

                return NotFound("Settings file not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting settings.");
                return StatusCode(500, "Error resetting settings.");
            }
        }

        [HttpGet("validate")]
        public IActionResult ValidateSettings()
        {
            try
            {
                var settings = ReadSettingsFromFile();
                if (string.IsNullOrWhiteSpace(settings.Setting1) || string.IsNullOrWhiteSpace(settings.Setting2))
                {
                    return BadRequest("Settings are incomplete or invalid.");
                }

                return Ok(new { message = "Settings are valid." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating settings.");
                return StatusCode(500, "Error validating settings.");
            }
        }

        // Helper method to read settings from a JSON file
        private SettingsModel ReadSettingsFromFile()
        {
            if (System.IO.File.Exists(_settingsFilePath))
            {
                var json = System.IO.File.ReadAllText(_settingsFilePath);
                return JsonConvert.DeserializeObject<SettingsModel>(json);
            }

            return new SettingsModel(); // Return default settings if the file doesn't exist
        }

        // Helper method to save settings to a JSON file
        private void SaveSettingsToFile(SettingsModel settings)
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            System.IO.File.WriteAllText(_settingsFilePath, json);
        }
    }

    // Settings model
    public class SettingsModel
    {
        public string Setting1 { get; set; }
        public string Setting2 { get; set; }
    }
}
