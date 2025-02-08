using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json; // For JSON serialization
using System; // For DateTime

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

                var archiveFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"logs_archive_{DateTime.Now:yyyyMMddHHmmss}.txt"); // Timestamped archive filename
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
                return File(fileBytes, "application/octet-stream", "logs.txt"); // Return file for download
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading logs from file: {LogFilePath}", _logFilePath);
                return StatusCode(500, $"Error downloading logs: {ex.Message}");
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

