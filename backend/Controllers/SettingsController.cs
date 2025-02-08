using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations; // For data validation

namespace QuantumCrossScripting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SettingsController> _logger;
        private readonly string _settingsFilePath;

        public SettingsController(IConfiguration configuration, ILogger<SettingsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
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
            if (!ModelState.IsValid) // Using Data Annotations for Validation
            {
                return BadRequest(ModelState);
            }

            try
            {
                var settings = new SettingsModel
                {
                    Setting1 = model.Setting1,
                    Setting2 = model.Setting2,
                    Setting3 = model.Setting3 // Added new Setting3
                };

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

        // ... (rest of the code remains the same)

        // Settings model with Data Annotations for validation
        public class SettingsModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "Setting1 cannot be longer than 100 characters.")]
            public string Setting1 { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Setting2 cannot be longer than 100 characters.")]
            public string Setting2 { get; set; }

            [RegularExpression(@"^[0-9]+$", ErrorMessage = "Setting3 must be a number.")] // Example validation
            public string Setting3 { get; set; } // Added new setting with validation
        }

        // ... (RestoreModel remains the same)
    }
}

