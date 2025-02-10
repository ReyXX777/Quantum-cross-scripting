using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var settings = new SettingsModel
                {
                    Setting1 = model.Setting1,
                    Setting2 = model.Setting2,
                    Setting3 = model.Setting3,
                    Setting4 = model.Setting4,
                    Setting5 = model.Setting5
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

        [HttpPost("backup")]
        public IActionResult BackupSettings()
        {
            try
            {
                if (!System.IO.File.Exists(_settingsFilePath))
                {
                    return NotFound("Settings file not found.");
                }

                var backupFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"settings_backup_{DateTime.Now:yyyyMMddHHmmss}.json");
                System.IO.File.Copy(_settingsFilePath, backupFilePath);
                _logger.LogInformation("Settings backed up successfully to {BackupFilePath}", backupFilePath);
                return Ok(new { message = "Settings backed up successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error backing up settings.");
                return StatusCode(500, "Error backing up settings.");
            }
        }

        [HttpPost("restore")]
        public IActionResult RestoreSettings([FromBody] RestoreModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.BackupFilePath))
            {
                return BadRequest("Backup file path is required.");
            }

            try
            {
                if (!System.IO.File.Exists(model.BackupFilePath))
                {
                    return NotFound("Backup file not found.");
                }

                System.IO.File.Copy(model.BackupFilePath, _settingsFilePath, overwrite: true);
                _logger.LogInformation("Settings restored successfully from {BackupFilePath}", model.BackupFilePath);
                return Ok(new { message = "Settings restored successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring settings.");
                return StatusCode(500, "Error restoring settings.");
            }
        }

        [HttpPost("validate")]
        public IActionResult ValidateSettings([FromBody] SettingsModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(new { message = "Settings are valid." });
        }

        [HttpPost("export")]
        public IActionResult ExportSettings()
        {
            try
            {
                var settings = ReadSettingsFromFile();
                var jsonSettings = JsonConvert.SerializeObject(settings);
                return Ok(new { ExportedSettings = jsonSettings });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting settings.");
                return StatusCode(500, "Error exporting settings.");
            }
        }

        [HttpPost("import")]
        public IActionResult ImportSettings([FromBody] ImportModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.JsonSettings))
            {
                return BadRequest("JSON settings are required.");
            }

            try
            {
                var settings = JsonConvert.DeserializeObject<SettingsModel>(model.JsonSettings);
                SaveSettingsToFile(settings);
                _logger.LogInformation("Settings imported successfully.");
                return Ok(new { message = "Settings imported successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing settings.");
                return StatusCode(500, "Error importing settings.");
            }
        }

        private SettingsModel ReadSettingsFromFile()
        {
            if (!System.IO.File.Exists(_settingsFilePath))
            {
                return new SettingsModel();
            }

            var json = System.IO.File.ReadAllText(_settingsFilePath);
            return JsonConvert.DeserializeObject<SettingsModel>(json);
        }

        private void SaveSettingsToFile(SettingsModel settings)
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            System.IO.File.WriteAllText(_settingsFilePath, json);
        }

        public class SettingsModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "Setting1 cannot be longer than 100 characters.")]
            public string Setting1 { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Setting2 cannot be longer than 100 characters.")]
            public string Setting2 { get; set; }

            [RegularExpression(@"^[0-9]+$", ErrorMessage = "Setting3 must be a number.")]
            public string Setting3 { get; set; }

            [EmailAddress(ErrorMessage = "Setting4 must be a valid email address.")]
            public string Setting4 { get; set; }

            [Range(1, 100, ErrorMessage = "Setting5 must be between 1 and 100.")]
            public int Setting5 { get; set; }
        }

        public class RestoreModel
        {
            public string BackupFilePath { get; set; }
        }

        public class ImportModel
        {
            public string JsonSettings { get; set; }
        }
    }
}

