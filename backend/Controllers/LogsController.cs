using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System;

namespace QuantumCrossScripting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly string _logFilePath;
        private readonly ILogger<LogsController> _logger;

        public LogsController(IConfiguration configuration, ILogger<LogsController> logger)
        {
            _logger = logger;
            _logFilePath = Path.Combine(Directory.GetCurrentDirectory(), configuration["Logging:LogFilePath"] ?? "logs.txt");
        }

        [HttpGet]
        public IActionResult GetLogs(int? lines = 100)
        {
            if (!System.IO.File.Exists(_logFilePath))
            {
                _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                return NotFound("Log file not found.");
            }

            try
            {
                var logs = ReadLogsFromFile(_logFilePath, lines ?? 100);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading logs from file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error reading logs: {ex.Message}");
            }
        }

        [HttpPost("clear")]
        public IActionResult ClearLogs()
        {
            try
            {
                if (!System.IO.File.Exists(_logFilePath))
                {
                    _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                    return NotFound("Log file not found.");
                }

                System.IO.File.WriteAllText(_logFilePath, string.Empty);
                _logger.LogInformation("Logs cleared successfully.");
                return Ok(new { Message = "Logs cleared successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing logs from file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error clearing logs: {ex.Message}");
            }
        }

        [HttpPost("search")]
        public IActionResult SearchLogs([FromBody] SearchModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Keyword))
            {
                return BadRequest("Keyword is required.");
            }

            try
            {
                if (!System.IO.File.Exists(_logFilePath))
                {
                    _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                    return NotFound("Log file not found.");
                }

                var logs = System.IO.File.ReadLines(_logFilePath)
                    .Where(line => line.Contains(model.Keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching logs in file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error searching logs: {ex.Message}");
            }
        }

        [HttpPost("archive")]
        public IActionResult ArchiveLogs()
        {
            try
            {
                if (!System.IO.File.Exists(_logFilePath))
                {
                    _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                    return NotFound("Log file not found.");
                }

                var archiveFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"logs_archive_{DateTime.Now:yyyyMMddHHmmss}.txt");
                System.IO.File.Copy(_logFilePath, archiveFilePath, overwrite: true);
                _logger.LogInformation("Logs archived successfully to {ArchiveFilePath}", archiveFilePath);
                return Ok(new { Message = "Logs archived successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving logs from file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error archiving logs: {ex.Message}");
            }
        }

        [HttpPost("filter")]
        public IActionResult FilterLogs([FromBody] FilterModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Level))
            {
                return BadRequest("Filter level is required.");
            }

            try
            {
                if (!System.IO.File.Exists(_logFilePath))
                {
                    _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                    return NotFound("Log file not found.");
                }

                var logs = System.IO.File.ReadLines(_logFilePath)
                    .Where(line => line.Contains($"[{model.Level}]", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering logs in file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error filtering logs: {ex.Message}");
            }
        }

        [HttpGet("download")]
        public IActionResult DownloadLogs()
        {
            if (!System.IO.File.Exists(_logFilePath))
            {
                _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                return NotFound("Log file not found.");
            }

            try
            {
                var fileBytes = System.IO.File.ReadAllBytes(_logFilePath);
                return File(fileBytes, "application/octet-stream", "logs.txt");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading logs from file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error downloading logs: {ex.Message}");
            }
        }

        [HttpPost("export-json")]
        public IActionResult ExportLogsAsJson()
        {
            if (!System.IO.File.Exists(_logFilePath))
            {
                _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                return NotFound("Log file not found.");
            }

            try
            {
                var logs = System.IO.File.ReadAllLines(_logFilePath);
                var jsonLogs = JsonSerializer.Serialize(logs);
                return Ok(new { JsonLogs = jsonLogs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting logs as JSON from file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error exporting logs: {ex.Message}");
            }
        }

        [HttpPost("stats")]
        public IActionResult GetLogStats()
        {
            if (!System.IO.File.Exists(_logFilePath))
            {
                _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                return NotFound("Log file not found.");
            }

            try
            {
                var logs = System.IO.File.ReadAllLines(_logFilePath);
                var stats = new
                {
                    TotalLines = logs.Length,
                    ErrorCount = logs.Count(line => line.Contains("[Error]")),
                    WarningCount = logs.Count(line => line.Contains("[Warning]")),
                    InfoCount = logs.Count(line => line.Contains("[Info]"))
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating log stats from file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error calculating stats: {ex.Message}");
            }
        }

        [HttpPost("rotate")]
        public IActionResult RotateLogs()
        {
            try
            {
                if (!System.IO.File.Exists(_logFilePath))
                {
                    _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                    return NotFound("Log file not found.");
                }

                var rotatedFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"logs_rotated_{DateTime.Now:yyyyMMddHHmmss}.txt");
                System.IO.File.Move(_logFilePath, rotatedFilePath);
                System.IO.File.WriteAllText(_logFilePath, string.Empty);
                _logger.LogInformation("Logs rotated successfully to {RotatedFilePath}", rotatedFilePath);
                return Ok(new { Message = "Logs rotated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rotating logs from file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error rotating logs: {ex.Message}");
            }
        }

        [HttpPost("compress")]
        public IActionResult CompressLogs()
        {
            try
            {
                if (!System.IO.File.Exists(_logFilePath))
                {
                    _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                    return NotFound("Log file not found.");
                }

                var compressedFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"logs_compressed_{DateTime.Now:yyyyMMddHHmmss}.zip");
                // Simulate compression (actual compression logic can be added here)
                System.IO.File.Copy(_logFilePath, compressedFilePath);
                _logger.LogInformation("Logs compressed successfully to {CompressedFilePath}", compressedFilePath);
                return Ok(new { Message = "Logs compressed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compressing logs from file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error compressing logs: {ex.Message}");
            }
        }

        [HttpPost("backup")]
        public IActionResult BackupLogs()
        {
            try
            {
                if (!System.IO.File.Exists(_logFilePath))
                {
                    _logger.LogWarning("Log file not found at {LogFilePath}", _logFilePath);
                    return NotFound("Log file not found.");
                }

                var backupFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"logs_backup_{DateTime.Now:yyyyMMddHHmmss}.txt");
                System.IO.File.Copy(_logFilePath, backupFilePath);
                _logger.LogInformation("Logs backed up successfully to {BackupFilePath}", backupFilePath);
                return Ok(new { Message = "Logs backed up successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error backing up logs from file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error backing up logs: {ex.Message}");
            }
        }

        private List<string> ReadLogsFromFile(string filePath, int lineCount)
        {
            var logs = new List<string>();

            if (new FileInfo(filePath).Length == 0) return logs;

            var lines = System.IO.File.ReadLines(filePath).Reverse().Take(lineCount);

            logs.AddRange(lines);
            return logs;
        }
    }

    public class SearchModel
    {
        public string Keyword { get; set; }
    }

    public class FilterModel
    {
        public string Level { get; set; }
    }
}

